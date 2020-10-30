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

        [Parameter]
        public EventCallback<Bottleholder> BottleholderSelectEvent { get; set; }

        public string markedText;

        public List<Bottleholder> Bottleholders { get; set; }

        public Bottleholder UserSelectedBottleHolder = null;

        private BeerliftMessage _lastBeerliftMessage = null;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _telemetryService.InputMessageReceived += OnInputTelemetryReceived;

            Bottleholders = _sqliteService.GetBottleHolders(deviceId, moduleName);
        }

        void IDisposable.Dispose()
        {
            _telemetryService.InputMessageReceived -= OnInputTelemetryReceived;
        }

        public void AddBottle()
        {
        }

        private async Task MarkPosition()
        {
            markedText = string.Empty;

            if (UserSelectedBottleHolder == null)
            {
                return;
            }

            markedText = "marking...";

            await _ioTHubServiceClientService.SendDirectMethod<MarkPositionRequest, MarkPositionResponse>(deviceId, moduleName, "MarkPosition", new MarkPositionRequest { position = UserSelectedBottleHolder.id });

            markedText = $"marked {UserSelectedBottleHolder.id}";
        }

        public async Task BottleHolderSelected(ChangeEventArgs args)
        {
            UserSelectedBottleHolder = (from x in Bottleholders
                                        where x.id.ToString() == args.Value.ToString()
                                        select x).First();

            BottleholderSelectEvent.InvokeAsync(UserSelectedBottleHolder).Wait();

            await MarkPosition();
        }

        private async void OnInputTelemetryReceived(object sender, BeerliftMessage message)
        {
            if (message == null
                    || message.deviceId != deviceId)
            {
                return;
            }

            ProcessChanges(_lastBeerliftMessage, message);

            _lastBeerliftMessage = message;

            await InvokeAsync(() => StateHasChanged());
        }

        private void ProcessChanges(BeerliftMessage lastBeerliftMessage, BeerliftMessage message)
        {
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot01 != message.slot01)
            {
                Bottleholders[0].state = message.slot01 ? "occupied" : "      ";
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot02 != message.slot02)
            {
                Bottleholders[1].state = message.slot02 ? "occupied" : "      ";
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot03 != message.slot03)
            {
                Bottleholders[2].state = message.slot03 ? "occupied" : "      ";
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot04 != message.slot04)
            {
                Bottleholders[3].state = message.slot04 ? "occupied" : "      ";
            }

            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot05 != message.slot05)
            {
                Bottleholders[4].state = message.slot05 ? "occupied" : "      ";
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot06 != message.slot06)
            {
                Bottleholders[5].state = message.slot06 ? "occupied" : "      ";
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot07 != message.slot07)
            {
                Bottleholders[6].state = message.slot07 ? "occupied" : "      ";
            }
            if (lastBeerliftMessage == null
                    || lastBeerliftMessage.slot08 != message.slot08)
            {
                Bottleholders[7].state = message.slot08 ? "occupied" : "      ";
            }
        }
    }
}