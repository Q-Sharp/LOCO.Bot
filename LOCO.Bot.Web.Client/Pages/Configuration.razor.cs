namespace LOCO.Bot.Web.Client.Pages;

public partial class Configuration
{
    [Inject]
    public IAuthorizedClientFactory HttpClientFactory { get; set; }

    [Inject]
    public ISnackbar SnackBar { get; set; }

    [Inject] 
    public IDialogService DialogService { get; set; }

    private MudColor _selectedColor;

    private readonly List<string> _editEvents = new();

    private WheelEntry _selectedItem = null;
    private WheelEntry _elementBeforeEdit;

    private List<WheelEntry> _wheelEntries = new();

    protected override async Task OnInitializedAsync() => await LoadEntries();

    private async Task LoadEntries()
    {
        var httpClient = HttpClientFactory.CreateClient();
        _wheelEntries = new List<WheelEntry>();

        var we = await httpClient.GetFromJsonAsync<IEnumerable<WheelEntry>>("api/wheel");
        _wheelEntries.AddRange(we);
    }

    private void BackupItem(object item)
    {
        if (item is WheelEntry wheelEntry)
        {
            _selectedColor = new MudColor(wheelEntry.Color);

            _elementBeforeEdit = new()
            {
                Id = wheelEntry.Id,
                Color = wheelEntry.Color,
                Text = wheelEntry.Text,
                Qty = wheelEntry.Qty
            };
        }
    }

    private void AddNew()
    {
        var we = new WheelEntry { Color = "#000000", Text = string.Empty, Qty = 0, Id = 0 };
        _wheelEntries.Add(we);
        _selectedItem = we;
        StateHasChanged();
    }

    private async void Delete()
    {
        if (_selectedItem is not null)
        {
            var id = _selectedItem.Id;

            if (id == 0)
                _wheelEntries.Remove(_selectedItem);
            else
            {
                var answer = await DialogService.ShowMessageBox("ORLY", $"The wheel segment: \"{_selectedItem.Text}\" will get deleted.", cancelText: "Cancel");

                if (answer ?? false)
                {
                    var httpClient = HttpClientFactory.CreateClient();
                    await httpClient.DeleteAsync($"api/wheel/delete/{id}");
                    await LoadEntries();
                }
            }
        }
    }

    private async void ItemHasBeenCommitted(object item)
    {
        if (item is WheelEntry wheelEntry)
        {
            var httpClient = HttpClientFactory.CreateClient();
            await httpClient.PostAsJsonAsync<WheelEntry>("api/wheel", wheelEntry);

            await LoadEntries();
        }
    }

    private void ResetItemToOriginalValues(object item)
    {
        if (item is WheelEntry wheelEntry)
        {
            if (_elementBeforeEdit.Id == 0)
            {
                _wheelEntries.Remove(wheelEntry);
                StateHasChanged();
            }
            else
            {
                wheelEntry.Id = _elementBeforeEdit.Id;
                wheelEntry.Color = _elementBeforeEdit.Color;
                wheelEntry.Text = _elementBeforeEdit.Text;
                wheelEntry.Qty = _elementBeforeEdit.Qty;
            }
        }
    }
}
