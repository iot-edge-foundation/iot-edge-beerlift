using BeerliftDashboard.Data;
using IoTEdgeConversationDashboard.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeerliftDashboard
{
    public class IndexBase : ComponentBase, IDisposable
    {
        public string message;

        public double temperature;

        public double humidity;

        public bool flooded;

        public int attempts;

        public string liftState;

        public string deviceId
        {
            get
            {
                return _sqliteService.ReadSetting("deviceId");
            }
            set
            {
                _sqliteService.WriteSetting("deviceId", value);
            }
        }

        public string moduleName
        {
            get
            {
                return _sqliteService.ReadSetting("moduleName");
            }
            set
            {
                _sqliteService.WriteSetting("moduleName", value);
            }
        }

        public int emptySlot;

        public int position;

        public string telemetryMessage;

        [Inject]
        public IoTHubServiceClientService _ioTHubServiceClientService { get; set; }

        [Inject]
        public SqliteService _sqliteService { get; set; }

        [Inject]
        public TelemetryService _telemetryService { get; set; }

        protected override void OnInitialized()
        {
            message = _sqliteService.GetVersion();

            _telemetryService.InputMessageReceived += OnInputMessageReceived;
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
                emptySlot = response.FindEmptySlotPayload.emptySlot;
            }
        }

        public async Task MarkPosition()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<MarkPositionRequest, MarkPositionResponse>(deviceId, moduleName, "MarkPosition", new MarkPositionRequest { position = position });
        }

        private async void OnInputMessageReceived(object sender, string messageString)
        {
            telemetryMessage = messageString;

            await InvokeAsync(() => StateHasChanged());
        }
    }
}