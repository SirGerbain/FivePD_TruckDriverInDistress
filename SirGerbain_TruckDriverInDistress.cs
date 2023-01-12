using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;

namespace SirGerbain_TruckDriverInDistress
{
    [CalloutProperties("Truck Driver in distress", "sirGerbain", "1.0.0")]
    public class SirGerbain_TruckDriverInDistress : FivePD.API.Callout
    {
        Ped truckDriver;
        Vehicle truck, trailer;
        Vector3 spawnPlace;
        List<Vehicle> kurumas = new List<Vehicle>();
        List<Ped> kurumaDrivers = new List<Ped>();
        List<Ped> kurumaPassengers = new List<Ped>();
        Random random = new Random();

        public SirGerbain_TruckDriverInDistress()
        {
            Vector3[] freewayLocations = {
                new Vector3(1308.065f,581.2718f,79.78977f),
                new Vector3(1594.56f,1009.64f,78.95673f),
                new Vector3(1689.932f,1352.383f,87.02444f),
                new Vector3(1878.03f,2336.891f,55.68655f),
                new Vector3(2061.051f,2644.353f,52.16157f),
                new Vector3(2360.496f,2856.932f,40.1833f),
                new Vector3(2539.596f,3042.605f,42.92778f),
                new Vector3(2946.281f,3813.16f,52.26584f),
                new Vector3(2795.884f,446.548f,48.0796f),
                new Vector3(2649.814f,4928.652f,44.39455f),
                new Vector3(2331.14f,5905.173f,47.67876f),
                new Vector3(1436.997f,6474.696f,20.40421f),
                new Vector3(777.1246f,6513.02f,24.64042f),
                new Vector3(-589.123f,5663.769f,38.00635f),
                new Vector3(-1529.918f,4981.798f,62.087722f),
                new Vector3(-2329.675f,4112.701f,35.33438f),
                new Vector3(-589.123f,5663.769f,38.00635f),
                new Vector3(-1529.918f,4981.798f,62.08722f),
                new Vector3(-2329.675f,4112.701f,35.33438f),
                new Vector3(-2620.146f,2824.454f,16.38638f),
                new Vector3(-3039.727f,1872.351f,29.84845f),
                new Vector3(-3128.839f,835.1783f,16.17631f),
                new Vector3(-2539.692f,-185.8579f,19.42014f),
                new Vector3(-1842.228f,-595.9995f,11.09579f)
            };

            spawnPlace = freewayLocations[random.Next(0, 22)];

            InitInfo(spawnPlace);
            ShortName = "Truck Driver in distress";
            CalloutDescription = "All units: a truck driver called in to report suspicious vehicles chasing him.";
            ResponseCode = 3;
            StartDistance = 500f;

        }

        public async override Task OnAccept()
        {
            InitBlip();
            UpdateData();

            // Spawn truck and trailer
            truckDriver = await SpawnPed(PedHash.Trucker01SMM, Location);
            truckDriver.AlwaysKeepTask = true;
            truckDriver.BlockPermanentEvents = true;
            truck = await SpawnVehicle(VehicleHash.Hauler, Location, 0f);
            truck.AttachBlip();
            trailer = await SpawnVehicle(VehicleHash.Trailers4, Location);
            API.AttachVehicleToTrailer(truck.GetHashCode(), trailer.GetHashCode(), 1F);
            truckDriver.SetIntoVehicle(truck, VehicleSeat.Driver);

            for (int i = 0; i < 3; i++)
            {

                float offsetX = 5.0f * (float)Math.Cos(i * 120.0f * (Math.PI / 180.0)); // offset to position the Kuruma's around the truck
                float offsetY = 5.0f * (float)Math.Sin(i * 120.0f * (Math.PI / 180.0));
                Vector3 kurumaPosition = Location + new Vector3(offsetX, offsetY, 0);
                Vehicle kuruma = await SpawnVehicle(VehicleHash.Kuruma, kurumaPosition);
                kuruma.Mods.PrimaryColor = VehicleColor.MetallicBlack;
                kuruma.Mods.SecondaryColor = VehicleColor.MetallicBlack;
                kuruma.Mods.PearlescentColor = VehicleColor.MetallicBlack;
                kuruma.Mods.HasNeonLight(VehicleNeonLight.Right);
                kuruma.Mods.HasNeonLight(VehicleNeonLight.Left);
                kuruma.Mods.HasNeonLight(VehicleNeonLight.Back);
                kuruma.Mods.HasNeonLight(VehicleNeonLight.Front);
                kuruma.Mods.SetNeonLightsOn(VehicleNeonLight.Right, true);
                kuruma.Mods.SetNeonLightsOn(VehicleNeonLight.Left, true);
                kuruma.Mods.SetNeonLightsOn(VehicleNeonLight.Back, true);
                kuruma.Mods.SetNeonLightsOn(VehicleNeonLight.Front, true);
                kuruma.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(100, 0, 255, 0);

                Ped kurumaDriver = await SpawnPed(PedHash.Korean02GMY, Location);
                kurumaDriver.AlwaysKeepTask = true;
                kurumaDriver.BlockPermanentEvents = true;
                kurumaDriver.SetIntoVehicle(kuruma, VehicleSeat.Driver);

                Ped kurumaPassenger = await SpawnPed(PedHash.Korean02GMY, Location);
                kurumaPassenger.AlwaysKeepTask = true;
                kurumaPassenger.BlockPermanentEvents = true;
                kurumaPassenger.SetIntoVehicle(kuruma, VehicleSeat.Passenger);
                kurumaPassenger.Weapons.Give(WeaponHash.Pistol, 100, true, true);

                kurumas.Add(kuruma);
                kurumaDrivers.Add(kurumaDriver);
                kurumaPassengers.Add(kurumaPassenger);
            }

        }

        public async override void OnStart(Ped player)
        {
            base.OnStart(player);

            truckDriver.Task.CruiseWithVehicle(truck, 100f, 524828);
            for (int i = 0; i < kurumas.Count; i++)
            {
                kurumaDrivers[i].Task.VehicleChase(truckDriver);
                kurumas[i].AttachBlip();
            }

            while (true)
            {
                await BaseScript.Delay(random.Next(10000, 23000));
                int theChosenOne = random.Next(0, kurumaPassengers.Count);
                kurumaPassengers[theChosenOne].Task.VehicleShootAtPed(truckDriver);
                await BaseScript.Delay(4000);
                kurumaPassengers[theChosenOne].Task.ClearAll();
            }

        }

    }
}

