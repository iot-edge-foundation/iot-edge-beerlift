using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using IoTEdgeConversationDashboard.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeerliftDashboard
{
    public class BeerliftBase : ComponentBase, IDisposable
    {
        [Inject]
        public IoTHubServiceClientService _ioTHubServiceClientService { get; set; }

        [Inject]
        public SqliteService _sqliteService { get; set; }

        [Inject]
        public TelemetryService _telemetryService { get; set; }

        [Inject]
        public SessionService _sessionService { get; set; }

        public double temperature;

        public double humidity;

        public bool flooded;

        public int attempts;

        public string liftState;

        public string password;

        public int emptySlotId;

        public string telemetryMessage;

        public string message;

        public string deviceId { get; set; }

        public string moduleName { get; set; }

        public List<Bottleholder> Bottleholders { get; set; }

        protected override void OnInitialized()
        {
            _telemetryService.InputMessageReceived += OnInputMessageReceived;

            deviceId = _sqliteService.ReadSetting("deviceId");

            moduleName = _sqliteService.ReadSetting("moduleName");
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            InitializeBeerliftTable();

            Bottleholders = _sqliteService.GetBottleHolders(deviceId, moduleName);
        }

        void IDisposable.Dispose()
        {
            _telemetryService.InputMessageReceived -= OnInputMessageReceived;
        }

        public async Task Up()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<UpRequest, UpResponse>(deviceId, moduleName, "Up", new UpRequest());
        }

        public async Task Down()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<DownRequest, DownResponse>(deviceId, moduleName, "Down", new DownRequest());
        }

        public async Task Circus()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<CircusRequest, CircusResponse>(deviceId, moduleName, "Circus", new CircusRequest());
        }

        public async Task Advertise()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<AdvertiseRequest, AdvertiseResponse>(deviceId, moduleName, "Advertise", new AdvertiseRequest());
        }

        public async Task Ambiant()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<AmbiantRequest, AmbiantResponse>(deviceId, moduleName, "Ambiant", new AmbiantRequest());

            if (response.ResponseStatus == 200)
            {
                temperature = response.AmbiantPayload.temperature;
                humidity = response.AmbiantPayload.humidity;
                flooded = response.AmbiantPayload.flooded;
                attempts = response.AmbiantPayload.attempts;
                liftState = response.AmbiantPayload.liftState;
            }
        }

        public async Task FindEmptySlot()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<FindEmptySlotRequest, FindEmptySlotResponse>(deviceId, moduleName, "FindEmptySlot", new FindEmptySlotRequest());

            if (response.ResponseStatus == 200)
            {
                emptySlotId = response.FindEmptySlotPayload.emptySlot;
            }
        }

        private async void OnInputMessageReceived(object sender, string messageString)
        {
            telemetryMessage = messageString;

            await InvokeAsync(() => StateHasChanged());
        }

        public bool Validated
        {
            get
            {
                return _sessionService.Validated;
            }
        }

        private void InitializeBeerliftTable()
        {
            if (!string.IsNullOrEmpty(deviceId)
                    && !string.IsNullOrEmpty(moduleName))
            {
                _sqliteService.IntializeBeerlift(deviceId, moduleName);
            }
        }

        public void BottleholderSelected(Bottleholder bottleholder)
        {
            message = $"Event Raised. bottleholder Selected: {bottleholder.name} with id {bottleholder.id}";
        }
    }
}