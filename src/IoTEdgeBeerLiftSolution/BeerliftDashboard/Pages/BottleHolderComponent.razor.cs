using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using IoTEdgeConversationDashboard.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BeerliftDashboard.Pages
{
    public class BottleHolderComponentBase : ComponentBase, IDisposable
    {
        private BeerliftMessage _lastBeerliftMessage = null;

        [Inject]
        public BusyService _busyService { get; set; }

        [Inject]
        public IoTHubServiceClientService _ioTHubServiceClientService { get; set; }

        [Inject]
        public TelemetryService _telemetryService { get; set; }

        [Inject]
        public SqliteService _sqliteService { get; set; }

        [Parameter]
        public string deviceId { get; set; }

        [Parameter]
        public string moduleName { get; set; }

        public string BottleActionText;

        public List<Bottleholder> Bottleholders;

        public string BottleBrandAndMake;

        public Bottleholder selectedBottleHolder = null;

        public bool collapse1Visible;

        public bool disabled = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _telemetryService.InputMessageReceived += OnInputTelemetryReceived;

            _busyService.BusyEvent += _busyService_BusyEvent;

            Bottleholders = _sqliteService.GetBottleHolders(deviceId, moduleName);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                // execute with async pattern
                Action act = async () =>
                {
                    var beerHoldersResponse = await _ioTHubServiceClientService.SendDirectMethod<BottleHoldersRequest, BottleHoldersResponse>(deviceId, moduleName, "BottleHolders", new BottleHoldersRequest());

                    if (beerHoldersResponse.ResponseStatus == 200)
                    {
                        await _telemetryService.SendTelemetry(beerHoldersResponse.BeerHoldersPayload.BeerLiftMessage);
                    }
                };

                act();
            }
        }

        void IDisposable.Dispose()
        {
            _busyService.BusyEvent -= _busyService_BusyEvent;

            _telemetryService.InputMessageReceived -= OnInputTelemetryReceived;
        }

        private void _busyService_BusyEvent(object sender, bool busy)
        {
            disabled = busy;

            InvokeAsync(() => StateHasChanged()).Wait();
        }

        public async Task AddBottle()
        {
            _busyService.SetBusy(true);

            try
            {
                var emptySlotId = 0;

                BottleActionText = string.Empty;

                if (string.IsNullOrEmpty(BottleBrandAndMake))
                {
                    BottleActionText = "Enter brand and make";
                    return;
                }

                BottleActionText = "Searching for empty slot...";

                var response = await _ioTHubServiceClientService.SendDirectMethod<FindEmptySlotRequest, FindEmptySlotResponse>(deviceId, moduleName, "FindEmptySlot", new FindEmptySlotRequest());

                if (response.ResponseStatus == 200)
                {
                    emptySlotId = response.FindEmptySlotPayload.emptySlot;
                }

                if (emptySlotId == 0)
                {
                    BottleActionText = "No empty slot available ";

                    return;
                }

                BottleActionText = $"Found empty slot {emptySlotId}, Place the bottle";

                await InvokeAsync(() => StateHasChanged());

                var placed = false;

                var i = 0;

                while (!placed)
                {
                    if (i == 4)
                    {
                        // Max 4 attempts.
                        break;
                    }

                    i++;

                    BottleActionText = $"Found empty slot {emptySlotId}, Place the bottle... ({i})";

                    await InvokeAsync(() => StateHasChanged());

                    var beerHoldersResponse = await _ioTHubServiceClientService.SendDirectMethod<BottleHoldersRequest, BottleHoldersResponse>(deviceId, moduleName, "BottleHolders", new BottleHoldersRequest());

                    if (beerHoldersResponse.ResponseStatus == 200)
                    {
                        await _telemetryService.SendTelemetry(beerHoldersResponse.BeerHoldersPayload.BeerLiftMessage);

                        _lastBeerliftMessage = beerHoldersResponse.BeerHoldersPayload.BeerLiftMessage;

                        if (_lastBeerliftMessage.IsSlotInUse(emptySlotId))
                        {
                            // The correct bottle is placed
                            placed = true;
                            break;
                        }
                    }

                    await MarkPosition(emptySlotId);
                }

                if (placed)
                {
                    BottleActionText = $"Bottle is '{BottleBrandAndMake}' placed";

                    _sqliteService.PutBottleHolder(deviceId, moduleName, emptySlotId, BottleBrandAndMake, "occupied");

                    Bottleholders = _sqliteService.GetBottleHolders(deviceId, moduleName);

                    BottleBrandAndMake = string.Empty;
                }
                else
                {
                    BottleActionText = $"Timed out, please try again";
                }
            }
            finally
            {
                _busyService.SetBusy(false);
            }
        }

        private async Task MarkPosition(int position)
        {
            await _ioTHubServiceClientService.SendDirectMethod<MarkPositionRequest, MarkPositionResponse>(deviceId, moduleName, "MarkPosition", new MarkPositionRequest { position = position });
        }

        public async Task BottleHolderElementSelected(ChangeEventArgs args)
        {
            _busyService.SetBusy(true);

            BottleActionText = string.Empty;

            try
            {
                selectedBottleHolder = (from x in Bottleholders
                                        where x.indexer.ToString() == args.Value.ToString()
                                        select x).First();

                await MarkPosition(selectedBottleHolder.indexer);
            }
            finally
            {
                _busyService.SetBusy(false);
            }
        }

        private async void OnInputTelemetryReceived(object sender, BeerliftMessage message)
        {
            if (message == null
                    || message.deviceId != deviceId)
            {
                return;
            }

            var changedIndexers = ProcessChanges(_lastBeerliftMessage, message);

            foreach (var changedIndex in changedIndexers)
            {
                var bottleHolder = (from x in Bottleholders
                                    where x.indexer.ToString() == changedIndex.ToString()
                                    select x).First();

                var state = message.IsSlotInUse(changedIndex) ? "occupied" : "";

                _sqliteService.UpdateBottleHolderState(deviceId, moduleName, changedIndex, state);
            }

            Bottleholders = _sqliteService.GetBottleHolders(deviceId, moduleName);

            // Remember

            _lastBeerliftMessage = message;

            await InvokeAsync(() => StateHasChanged());
        }

        private List<int> ProcessChanges(BeerliftMessage lastBeerliftMessage, BeerliftMessage message)
        {
            var result = new List<int>();

            // bank A

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot01 != message.slot01)
            {
                Bottleholders[0].state = message.slot01 ? "occupied" : "      ";
                result.Add(1);
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot02 != message.slot02)
            {
                Bottleholders[1].state = message.slot02 ? "occupied" : "      ";
                result.Add(2);
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot03 != message.slot03)
            {
                Bottleholders[2].state = message.slot03 ? "occupied" : "      ";
                result.Add(3);
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot04 != message.slot04)
            {
                Bottleholders[3].state = message.slot04 ? "occupied" : "      ";
                result.Add(4);
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot05 != message.slot05)
            {
                Bottleholders[4].state = message.slot05 ? "occupied" : "      ";
                result.Add(5);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot06 != message.slot06)
            {
                Bottleholders[5].state = message.slot06 ? "occupied" : "      ";
                result.Add(6);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot07 != message.slot07)
            {
                Bottleholders[6].state = message.slot07 ? "occupied" : "      ";
                result.Add(7);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot08 != message.slot08)
            {
                Bottleholders[7].state = message.slot08 ? "occupied" : "      ";
                result.Add(8);
            }

            // bank B

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot09 != message.slot09)
            {
                Bottleholders[8].state = message.slot09 ? "occupied" : "      ";
                result.Add(9);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot10 != message.slot10)
            {
                Bottleholders[9].state = message.slot10 ? "occupied" : "      ";
                result.Add(10);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot11 != message.slot11)
            {
                Bottleholders[10].state = message.slot11 ? "occupied" : "      ";
                result.Add(11);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot12 != message.slot12)
            {
                Bottleholders[11].state = message.slot12 ? "occupied" : "      ";
                result.Add(12);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot13 != message.slot13)
            {
                Bottleholders[12].state = message.slot13 ? "occupied" : "      ";
                result.Add(13);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot14 != message.slot14)
            {
                Bottleholders[13].state = message.slot14 ? "occupied" : "      ";
                result.Add(14);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot15 != message.slot15)
            {
                Bottleholders[14].state = message.slot15 ? "occupied" : "      ";
                result.Add(15);
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot16 != message.slot16)
            {
                Bottleholders[15].state = message.slot16 ? "occupied" : "      ";
                result.Add(16);
            }

            return result;
        }

        public async Task RemoveBottle()
        {
            _busyService.SetBusy(true);

            try
            {
                _sqliteService.DropBottle(deviceId, moduleName, selectedBottleHolder.indexer);

                Bottleholders = _sqliteService.GetBottleHolders(deviceId, moduleName);

                BottleActionText = $"Bottle {selectedBottleHolder.indexer} is revoked";

                await MarkPosition(selectedBottleHolder.indexer);
            }
            finally
            {
                _busyService.SetBusy(false);
            }
        }
    }
}