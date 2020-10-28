using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using IoTEdgeConversationDashboard.Data;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeerliftDashboard.Pages
{
    public class BottleHolderComponentBase : ComponentBase
    {
        [Inject]
        public IoTHubServiceClientService _ioTHubServiceClientService { get; set; }

        [Inject]
        public SqliteService _sqliteService { get; set; }

        [Parameter]
        public List<Bottleholder> Bottleholders { get; set; }

        [Parameter]
        public string deviceId { get; set; }

        [Parameter]
        public string moduleName { get; set; }

        public Bottleholder selectedBottleHolder = null;

        [Parameter]
        public EventCallback<Bottleholder> BottleholderSelectEvent { get; set; }

        public string markedText;

        public async Task MarkPosition()
        {
            markedText = string.Empty;

            if (selectedBottleHolder == null)
            {
                return;
            }

            markedText = "marking...";

            await _ioTHubServiceClientService.SendDirectMethod<MarkPositionRequest, MarkPositionResponse>(deviceId, moduleName, "MarkPosition", new MarkPositionRequest { position = selectedBottleHolder.id });

            markedText = $"marked {selectedBottleHolder.id}";
        }

        public void BottleHolderSelected(ChangeEventArgs args)
        {
            selectedBottleHolder = (from x in Bottleholders
                                    where x.id.ToString() == args.Value.ToString()
                                    select x).First();

            BottleholderSelectEvent.InvokeAsync(selectedBottleHolder).Wait();
        }
    }
}