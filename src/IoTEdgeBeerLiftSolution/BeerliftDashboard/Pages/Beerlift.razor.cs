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

        private string _dbpassword;

        public List<Bottleholder> Bottleholders { get; set; }

        private string _deviceId;

        private string _moduleName;

        protected override void OnInitialized()
        {
            _dbpassword = _sqliteService.ReadSetting("password");

            _telemetryService.InputMessageReceived += OnInputMessageReceived;

            _deviceId = _sqliteService.ReadSetting("deviceId");

            _moduleName = _sqliteService.ReadSetting("moduleName");
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            InitializeBeerliftTable();

            Bottleholders = _sqliteService.GetBottleHolders(_deviceId, _moduleName);
        }

        void IDisposable.Dispose()
        {
            _telemetryService.InputMessageReceived -= OnInputMessageReceived;
        }

        public async Task Up()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<UpRequest, UpResponse>(_deviceId, _moduleName, "Up", new UpRequest());
        }

        public async Task Down()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<DownRequest, DownResponse>(_deviceId, _moduleName, "Down", new DownRequest());
        }

        public async Task Circus()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<CircusRequest, CircusResponse>(_deviceId, _moduleName, "Circus", new CircusRequest());
        }

        public async Task Advertise()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<AdvertiseRequest, AdvertiseResponse>(_deviceId, _moduleName, "Advertise", new AdvertiseRequest());
        }

        public async Task Ambiant()
        {
            var response = await _ioTHubServiceClientService.SendDirectMethod<AmbiantRequest, AmbiantResponse>(_deviceId, _moduleName, "Ambiant", new AmbiantRequest());

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
            var response = await _ioTHubServiceClientService.SendDirectMethod<FindEmptySlotRequest, FindEmptySlotResponse>(_deviceId, _moduleName, "FindEmptySlot", new FindEmptySlotRequest());

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
            if (!string.IsNullOrEmpty(_deviceId)
                    && !string.IsNullOrEmpty(_moduleName))
            {
                _sqliteService.IntializeBeerlift(_deviceId, _moduleName);
            }
        }

        public void BottleholderSelected(Bottleholder bottleholder)
        {
            message = $"Event Raised. bottleholder Selected: {bottleholder.name} with id {bottleholder.id}";
        }
    }
}