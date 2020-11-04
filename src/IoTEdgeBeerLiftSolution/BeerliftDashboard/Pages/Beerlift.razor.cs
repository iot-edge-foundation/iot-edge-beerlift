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
        public BusyService _busyService { get; set; }

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

        public bool disabled = false;
        public bool disabledUp = false;
        public bool disabledDown = false;

        public bool NoHeartBeat = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _telemetryService.InputMessageReceived += OnInputTelemetryReceived;

            _heartbeatService.InputMessageReceived += OnInputHeartbeatReceived;

            deviceId = _sqliteService.ReadSetting("deviceId");

            moduleName = _sqliteService.ReadSetting("moduleName");

            if (_sessionService.BeerliftMessage != null)
            {
                telemetryMessage = $"Updated at {_sessionService.BeerliftMessage.timeStamp}";
            }

            if (_sessionService.HeartbeatMessage != null)
            {
                heartbeatMessage = $"Updated at {_sessionService.HeartbeatMessage.timeStamp} ({_sessionService.HeartbeatMessage.counter})";
            }

            _busyService.BusyEvent += _busyService_BusyEvent;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                // execute with async pattern
                Action act = async () =>
                {
                    await Ambiant();
                };

                act();
            }
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            InitializeBeerliftTable();
        }

        void IDisposable.Dispose()
        {
            _busyService.BusyEvent -= _busyService_BusyEvent;

            _heartbeatService.InputMessageReceived -= OnInputHeartbeatReceived;

            _telemetryService.InputMessageReceived -= OnInputTelemetryReceived;
        }

        private void _busyService_BusyEvent(object sender, bool busy)
        {
            // when busy is TRUE all controls are disabled

            disabled = busy;

            disabledUp = busy |
                                  (liftState == "Up");

            disabledDown = busy |
                                  (liftState == "Down");

            InvokeAsync(() => StateHasChanged()).Wait();
        }

        public async Task Up()
        {
            _busyService.SetBusy(true);
            try
            {
                var response = await _ioTHubServiceClientService.SendDirectMethod<UpRequest, UpResponse>(deviceId, moduleName, "Up", new UpRequest());
            }
            finally
            {
                _busyService.SetBusy(false);
            }
        }

        public async Task Down()
        {
            _busyService.SetBusy(true);
            try
            {
                var response = await _ioTHubServiceClientService.SendDirectMethod<DownRequest, DownResponse>(deviceId, moduleName, "Down", new DownRequest());
            }
            finally
            {
                _busyService.SetBusy(false);
            }
        }

        public async Task Circus()
        {
            _busyService.SetBusy(true);
            try
            {
                var response = await _ioTHubServiceClientService.SendDirectMethod<CircusRequest, CircusResponse>(deviceId, moduleName, "Circus", new CircusRequest());
            }
            finally
            {
                _busyService.SetBusy(false);
            }
        }

        public async Task Advertise()
        {
            _busyService.SetBusy(true);
            try
            {
                var response = await _ioTHubServiceClientService.SendDirectMethod<AdvertiseRequest, AdvertiseResponse>(deviceId, moduleName, "Advertise", new AdvertiseRequest());
            }
            finally
            {
                _busyService.SetBusy(false);
            }
        }

        public async Task Ambiant()
        {
            _busyService.SetBusy(true);
            try
            {
                var response = await _ioTHubServiceClientService.SendDirectMethod<AmbiantRequest, AmbiantResponse>(deviceId, moduleName, "Ambiant", new AmbiantRequest());

                if (response.ResponseStatus == 200)
                {
                    temperature = Math.Round(response.AmbiantPayload.temperature, 1, MidpointRounding.AwayFromZero);
                    humidity = Math.Round(response.AmbiantPayload.humidity, 1, MidpointRounding.AwayFromZero);
                    flooded = response.AmbiantPayload.flooded;
                    attempts = response.AmbiantPayload.attempts;
                    liftState = response.AmbiantPayload.liftState;
                }
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

            // Message belongs to this beerlift

            _sessionService.BeerliftMessage = message;

            telemetryMessage = $"Updated at {message.timeStamp}";

            flooded = message.isFlooded;
            liftState = message.liftState;

            _busyService.SetBusy(null);

            await InvokeAsync(() => StateHasChanged());
        }

        private async void OnInputHeartbeatReceived(object sender, HeartbeatMessage message)
        {
            if (message == null
                    || (!string.IsNullOrEmpty(message.deviceId)
                                            && message.deviceId != deviceId))
            {
                return;
            }

            // Message belongs to this beerlift

            NoHeartBeat = message.elapsed;

            if (!NoHeartBeat)
            {
                _sessionService.HeartbeatMessage = message;

                heartbeatMessage = $"Updated at {message.timeStamp} ({message.counter})";
            }

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
    }
}