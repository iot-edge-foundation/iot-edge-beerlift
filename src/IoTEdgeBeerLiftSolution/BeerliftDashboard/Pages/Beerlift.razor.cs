using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using BeerliftDashboard.Pages;
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
        public HeartbeatService _heartbeatService { get; set; }

        [Inject]
        public SessionService _sessionService { get; set; }

        public double temperature;

        public double humidity;

        public bool flooded;

        public int attempts;

        public string liftState;

        public string password;

        public string telemetryMessage;

        public string heartbeatMessage;

        public string message;

        public string deviceId { get; set; }

        public string moduleName { get; set; }

        public List<Bottleholder> Bottleholders { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _telemetryService.InputMessageReceived += OnInputTelemetryReceived;

            _heartbeatService.InputMessageReceived += OnInputHeartbeatReceived;

            deviceId = _sqliteService.ReadSetting("deviceId");

            moduleName = _sqliteService.ReadSetting("moduleName");

            if (_sessionService.BeerliftMessage != null)
            {
                telemetryMessage = _sessionService.BeerliftMessage.ToString();
            }

            if (_sessionService.HeartbeatMessage != null)
            {
                heartbeatMessage = _sessionService.HeartbeatMessage.ToString();
            }
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            InitializeBeerliftTable();
        }

        void IDisposable.Dispose()
        {
            _heartbeatService.InputMessageReceived -= OnInputHeartbeatReceived;

            _telemetryService.InputMessageReceived -= OnInputTelemetryReceived;
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

        private async void OnInputTelemetryReceived(object sender, BeerliftMessage message)
        {
            if (message == null
                    || message.deviceId != deviceId)
            {
                return;
            }

            _sessionService.BeerliftMessage = message;

            telemetryMessage = message.ToString();

            await InvokeAsync(() => StateHasChanged());
        }

        private async void OnInputHeartbeatReceived(object sender, HeartbeatMessage message)
        {
            if (message == null
                    || message.deviceId != deviceId)
            {
                return;
            }

            _sessionService.HeartbeatMessage = message;

            heartbeatMessage = message.ToString();

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
            message = $"Event Raised. bottleholder Selected: {bottleholder.name} with index {bottleholder.indexer}";
        }
    }
}