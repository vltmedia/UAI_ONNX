using System;
using System.Collections.Generic;
using UAI.Common;
using UAI.Common.AI;
using UAI.UI;

public  class ONNXProcessors : UAIComponent
{
	
	public ONNXProcessorsConfig onnxProcessors;


    //public TMP_Dropdown menuBar;
    //   PopupMenu popupMenu;

    public Action<OnnxProcessor> OnOnnxProcessorFinishedEventHandler;
    public Action OnOnnxProcessorsFinishedEventHandler;
    
    public static ONNXProcessors Instance;

        public override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
        }
    }
    // Called when the node enters the scene tree for the first time.
    int currentIndex = 0;
    public bool isProcessing = false;
    public void Start()
    {
        currentIndex = 0;
        Awake();
        GenerateMenuItems();
        //PreviewManager.Instance.RunProcessClicked += () =>
        //{
        //    RunCurrentProcessor();
        //};
    }
    public void OnnxProcessorsFinished()
    {
        if (isProcessing)
        {
            isProcessing = false;
        //PreviewManager.Instance.processButton.Disabled = false;
        //PreviewManager.Instance.result.image.texture = OnnxAppState.processingOnnxProcessor.resultContainer.texture;
        OnOnnxProcessorsFinishedEventHandler.Invoke();
        }

    }

    public void OnnxProcessorFinished(OnnxProcessor processor)
    {
        if (isProcessing)
        {
            OnOnnxProcessorFinishedEventHandler.Invoke(processor);
            //EmitSignal(SignalName.OnOnnxProcessorsFinished, processor);
    }
    }

    private void GenerateMenuItems()
    {
        foreach (var processor in onnxProcessors.OnnxProcessors)
        {
            //popupMenu.AddItem(processor.catagoryName);
            //CreateNestedMenu(menuBar, processor.metadata.path);
            currentIndex++;
        }
    }

    //private void OnMenuItemSelected(int id)
    //{
    //    var item = onnxProcessors.OnnxProcessors[id];
    //        var nodePrefab = GD.Load<PackedScene>(item.onnxProcessor);
    //    OnnxProcessor onnxProcessor = (OnnxProcessor)nodePrefab.Instantiate();
    //    onnxProcessor.config = item;
    //    FXPanel.Instance.AddChildONNX(onnxProcessor);
    //    if(onnxProcessor.customOutputTab != null)
    //    {
    //        PreviewManager.Instance.AddTab($"Output: {item.metadata.name}", onnxProcessor.customOutputTab);
    //    }

    //}
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void Update()
	{
	}
    private void CreateNestedMenu(PopupMenu menuBar, string menuPath)
    {
        // Split the string by '/' to get the hierarchy
        string[] pathParts = menuPath.Split('/');

        // Start the recursive creation process
        AddMenuItems(menuBar, pathParts, 0);
    }
    // Recursive function to create the nested menus
    private void AddMenuItems(PopupMenu menuBar, string[] pathParts, int index)
    {
        if (index >= pathParts.Length)
            return;  // Base case: no more parts to process

        string currentPart = pathParts[index];

        // Try to find an existing MenuButton with the current part as its text
        //MenuButton menuButton = FindMenuButton(menuBar, currentPart);
        //PopupMenu popupMenu = null;
        //if (menuButton == null)
        //{
        //    // Create a new MenuButton if it doesn't exist
        //    popupMenu = new PopupMenu();
        //    popupMenu.Name = currentPart;

        //    // Add the MenuButton to the MenuBar
        //    menuBar.AddChild(popupMenu);
        //}

        //// Get the PopupMenu associated with the MenuButton
        ////PopupMenu popupMenu = menuButton.GetPopup();


        //// Recur into the next part if more path parts exist
        //if (index + 1 < pathParts.Length)
        //{
        //    AddSubmenu(popupMenu, pathParts, index + 1);
        //}
    }

    //private void AddSubmenu(PopupMenu parentMenu, string[] pathParts, int index)
    //{
    //    if (index >= pathParts.Length)
    //        return;  // Base case: no more parts to process
    //    bool isLast = index == pathParts.Length - 1;
    //    string currentPart = pathParts[index];

    //    // Check if this item already exists as a submenu by looking at the items in the parent menu
    //    int itemId = GetItemIdByText(parentMenu, currentPart);

    //    PopupMenu submenu;
    //    if (itemId == -1)
    //    {
    //        // Create a new submenu if it doesn't exist
    //        submenu = new PopupMenu();
    //        submenu.Name = currentPart;  // Set the name based on the current part
    //        //parentMenu.AddItem(currentPart);  // Add the submenu to the parent

    //        // Add the submenu to the menu button's popup
    //        if (!isLast)
    //        {
    //            parentMenu.AddChild(submenu);
    //            parentMenu.AddSubmenuNodeItem(currentPart, submenu);
    //        }
    //        else
    //        {
    //            parentMenu.AddItem(currentPart, currentIndex);
            
    //            parentMenu.IdPressed += (long index) =>
    //            {
    //                popupMenu = parentMenu;
    //                OnMenuItemSelected((int)index);
    //            };
    //        }
    //    }
    //    else
    //    {
    //        // If it already exists, retrieve the submenu (using the stored name)
    //        submenu = (PopupMenu)parentMenu.GetChild(itemId);
    //    }
    //    if (!isLast)
    //    {
    //        // Recur into the next part
    //        AddSubmenu(submenu, pathParts, index + 1);
    //}
    //}

    public async void RunCurrentProcessor()
    {
        if (!isProcessing)
        {
            isProcessing = true;
            UAIFunctionRuntimeResults.temp = UAIFunctionRuntimeResults.main;
            UAIFunctionRuntimeResults.main = new UAIFunctionResults();
            List<OnnxProcessor> fX = OnnxAppState.onnxProcessors;
            OnnxAppState.onnxProcessorsToProcess = fX;
            OnnxAppState.processingOnnxProcessor = fX[0];
            await OnnxAppState.processingOnnxProcessor.RunOnnxInference();
        }
    }
    
    public async void StartRunProcessors()
    {
        if (!isProcessing)
        {
            isProcessing = true;
            UAIFunctionRuntimeResults.temp = UAIFunctionRuntimeResults.main;
        UAIFunctionRuntimeResults.main = new UAIFunctionResults();
        List<OnnxProcessor> fX = OnnxAppState.onnxProcessors;
        OnnxAppState.onnxProcessorsToProcess = fX;
        await OnnxAppState.RunNextFX();
    }
    }

    // Helper function to find an existing MenuButton with the given text
    //private MenuButton FindMenuButton(MenuBar menuBar, string text)
    //{
    //    foreach (Node child in menuBar.GetChildren())
    //    {
    //        if (child is MenuButton menuButton && menuButton.Text == text)
    //        {
    //            return menuButton;
    //        }
    //    }
    //    return null;
    //}

    // Helper function to check if an item exists in the PopupMenu by its text
    //private int GetItemIdByText(PopupMenu menu, string text)
    //{
    //    for (int i = 0; i < menu.GetItemCount(); i++)
    //    {
    //        if (menu.GetItemText(i) == text)
    //        {
    //            return i;
    //        }
    //    }
    //    return -1; // Not found
    //}
}
