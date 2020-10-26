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
    public class IndexBase : ComponentBase, IDisposable
    {
        public string message;

        public double temperature;

        public double humidity;

        public bool flooded;

        public int attempts;

        public string liftState;

        public string password;

        public string _dbpassword;

        public string deviceId
        {
            get
            {
                return _sqliteService.ReadSetting("deviceId");
            }
            set
            {
                _sqliteService.WriteSetting("deviceId", value);
                InitializeBeerliftTable();
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
                InitializeBeerliftTable();
            }
        }

        public int emptySlot;

        public int position;

        public string telemetryMessage;

        [Parameter]
        public List<Bottleholder> Bottleholders { get; set; } = new List<Bottleholder>();

        [Inject]
        public IoTHubServiceClientService _ioTHubServiceClientService { get; set; }

        [Inject]
        public SqliteService _sqliteService { get; set; }

        [Inject]
        public TelemetryService _telemetryService { get; set; }

        protected override void OnInitialized()
        {
            message = _sqliteService.GetVersion();

            _dbpassword = _sqliteService.ReadSetting("password");

            _telemetryService.InputMessageReceived += OnInputMessageReceived;
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
            if (!CorrectPassword)
            {
                return;
            }

            var response = await _ioTHubServiceClientService.SendDirectMethod<UpRequest, UpResponse>(deviceId, moduleName, "Up", new UpRequest());
        }

        public async Task Down()
        {
            if (!CorrectPassword)
            {
                return;
            }

            var response = await _ioTHubServiceClientService.SendDirectMethod<DownRequest, DownResponse>(deviceId, moduleName, "Down", new DownRequest());
        }

        public async Task Circus()
        {
            if (!CorrectPassword)
            {
                return;
            }

            var response = await _ioTHubServiceClientService.SendDirectMethod<CircusRequest, CircusResponse>(deviceId, moduleName, "Circus", new CircusRequest());
        }

        public async Task Advertise()
        {
            if (!CorrectPassword)
            {
                return;
            }

            var response = await _ioTHubServiceClientService.SendDirectMethod<AdvertiseRequest, AdvertiseResponse>(deviceId, moduleName, "Advertise", new AdvertiseRequest());
        }

        public async Task Ambiant()
        {
            if (!CorrectPassword)
            {
                return;
            }

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
            if (!CorrectPassword)
            {
                return;
            }

            var response = await _ioTHubServiceClientService.SendDirectMethod<FindEmptySlotRequest, FindEmptySlotResponse>(deviceId, moduleName, "FindEmptySlot", new FindEmptySlotRequest());

            if (response.ResponseStatus == 200)
            {
                emptySlot = response.FindEmptySlotPayload.emptySlot;
            }
        }

        public async Task MarkPosition()
        {
            if (!CorrectPassword)
            {
                return;
            }

            var response = await _ioTHubServiceClientService.SendDirectMethod<MarkPositionRequest, MarkPositionResponse>(deviceId, moduleName, "MarkPosition", new MarkPositionRequest { position = position });
        }

        private async void OnInputMessageReceived(object sender, string messageString)
        {
            telemetryMessage = messageString;

            await InvokeAsync(() => StateHasChanged());
        }

        private bool CorrectPassword
        {
            get
            {
                return (password == _dbpassword)
                            && (!string.IsNullOrEmpty(password));
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