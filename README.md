# TitleBarDragRegions

## Use case
An abstraction layer to simplify the way that you can put interactive FrameworkElements on the TitleBar.

## Example

    public MainWindow()
    {
        this.InitializeComponent();
    
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    
        DragRegions = new DragRegions(this, AppTitleBar);
        DragRegions.NonDragElements = new FrameworkElement[] { SearchBox };
    }
