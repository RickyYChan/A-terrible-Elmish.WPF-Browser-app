module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF
open Elmish
open System.Windows
open System

[<AutoOpen>]
module IdList = 
    let len = Seq.length
    [<RequireQualifiedAccess>]
    module idlist = 
        let genId _ = 
            Guid.NewGuid() 
        let inline guid (x:^T) = 
            (^T : (member Guid : Guid) x)
        let inline rmAtId id = 
            List.filter (fun t -> guid t <> id)
        let inline mapAtId id f = 
            List.map (fun t -> if id = guid t then f t else t )
        let inline getIndexOfId id x = 
            if id = Guid.Empty 
            then -1 
            else List.findIndex(fun t -> id = guid t) x
        let inline getPreviousId id x = 
            let ind = x |> getIndexOfId id
            if ind = 0
            then if x.Length > 1 then x.[1] |> guid else Guid.Empty // ind = 0 need to be removed
            else if ind < 0 then Guid.Empty
            else x.[ind - 1] |> guid


[<AutoOpen>]
/// use static classes: System.Windows.Media.Brushes and System.Windows.Application.    
/// can use mkProgramWithCmdMsg as an alternative.   
/// seems dirty.  
module FwColor = 
    type Color = System.Windows.Media.Brushes
    let accent = System.Windows.Application.Current.Resources.["ImmersiveSystemAccentBrush"]
    let baseMediumLow = System.Windows.Application.Current.Resources.["SystemBaseMediumLowColorBrush"]
    let transparent = Color.Transparent |> box
    let baseLow = System.Windows.Application.Current.Resources.["SystemBaseLowColorBrush"]

[<AutoOpen>]
module Tab =
    let homePage = "https://www.bing.com"
    type TabItem = 
        {
            Guid: Guid
            Content: string 
            IsDisposed: bool 
            Address: string
            GoForwardCache: byte
            GoBackCache: byte
            ReloadCache: byte
        }
    let it = 
        {
            Guid = Guid.Empty
            Content = "Hello Elmish"
            IsDisposed = false
            Address = homePage
            GoForwardCache = 0uy 
            GoBackCache = 0uy
            ReloadCache = 0uy
        }
    type TabMsg = 
        | Dispose 
        | ChangeAddress of string
        | GoForward
        | GoBack
        | Reload
    let tabUpdate msg m = 
        match msg with 
        | Dispose -> { m with IsDisposed = true }
        | ChangeAddress uri -> { m with Address = uri; Content = uri |> Favicon.parse }
        | GoForward -> { m with GoForwardCache = m.GoForwardCache + 1uy }
        | GoBack -> { m with GoBackCache = m.GoBackCache + 1uy }
        | Reload -> { m with ReloadCache = m.ReloadCache + 1uy }

module App = 
    type Model =
        { 
            Tabs: Tab.TabItem list
            SelectionId: Guid
            MainWindowState: WindowState
            //MainWindow: Window
        }
    
    type Msg =
        | None
        | AddTab of string
        | SelectOfGuid of Guid
        | RmSelection
        | RmAfterDisposing
        | ChangeWindowState of WindowState
        | SetAddress of Guid * string
        | GoForward 
        | GoBack
        | Reload
    
    let init (*window*) =
        {
            Tabs = []
            SelectionId = Guid.Empty
            MainWindowState = WindowState.Normal
            //MainWindow = window
        }, Cmd.none
    
    let update msg m = 
        match msg with 
        | AddTab uri -> let id = idlist.genId()
                        { m with Tabs = m.Tabs @ [{ it with Content = uri |> Favicon.parse
                                                            Guid = id
                                                            Address = uri }] }, 
                        Cmd.ofMsg (SelectOfGuid id)
        | SelectOfGuid i -> { m with SelectionId = i }, Cmd.none
        | RmSelection -> { m with Tabs = m.Tabs |> idlist.mapAtId m.SelectionId (tabUpdate Dispose) }, 
                            Cmd.ofMsg RmAfterDisposing
        | RmAfterDisposing -> { m with Tabs = m.Tabs |> idlist.rmAtId m.SelectionId }, 
                                Cmd.ofMsg (SelectOfGuid (m.Tabs |> idlist.getPreviousId m.SelectionId))
        | ChangeWindowState ws -> { m with MainWindowState = ws }, Cmd.none
        | SetAddress (guid, uri) -> { m with Tabs = m.Tabs |> idlist.mapAtId guid (tabUpdate (ChangeAddress uri)) }, 
                                    Cmd.none
        | GoForward -> { m with Tabs = m.Tabs |> idlist.mapAtId m.SelectionId (tabUpdate TabMsg.GoForward) }, 
                        Cmd.none
        | GoBack -> { m with Tabs = m.Tabs |> idlist.mapAtId m.SelectionId (tabUpdate TabMsg.GoBack) }, 
                    Cmd.none
        | Reload -> { m with Tabs = m.Tabs |> idlist.mapAtId m.SelectionId (tabUpdate TabMsg.Reload) }, 
                    Cmd.none
        | None -> m, Cmd.none
    let bindings () : Binding<Model, Msg> list = 
        let changeState = function 
            | WindowState.Normal -> ChangeWindowState WindowState.Maximized
            | _ -> ChangeWindowState WindowState.Normal
        [
            "ReloadCmd" |> Binding.cmd (fun _ -> Reload)
            "ChangeStateCmd" |> Binding.cmd (fun m -> changeState m.MainWindowState)
            "MinimizationCmd" |> Binding.cmd (fun _ -> ChangeWindowState WindowState.Minimized)
            "ViewBorderThickness" |> Binding.oneWay(fun m -> 
                if m.Tabs.Length <> 0 && m.MainWindowState = WindowState.Maximized
                then 1 else 0
            )
            "GoBackCmd" |> Binding.cmd (fun _ -> GoBack)
            "GoForwardCmd" |> Binding.cmd (fun _ -> GoForward)
            "ExitApp" |> Binding.cmd (fun _ -> Environment.Exit 0; None)
            "MainWindowState" |> Binding.twoWay ((fun m -> m.MainWindowState), ChangeWindowState)
            "MouseLeftBtnDownCmd" |> Binding.cmdParam(fun e m -> 
                let args = e :?> System.Windows.Input.MouseButtonEventArgs
                match args.ClickCount with 
                | 1 -> (*m.MainWindow.DragMove()*)Application.Current.MainWindow.DragMove(); None
                | 2 -> changeState m.MainWindowState
                | _ -> None
            )
            "AddTabCmd" |> Binding.cmd (AddTab homePage)
            "RmTabCmd" |> Binding.cmd RmSelection
            "TabSource" |> Binding.subModelSeq(
                (fun m -> m.Tabs), 
                (fun t -> t.Guid), 
                (fun () ->  [
                        "TabContent" |> Binding.oneWay(fun (m, t) -> t.Content) 
                        "SelectItem" |> Binding.cmd(fun (m, t) -> SelectOfGuid t.Guid)
                        "TabBg" |> Binding.oneWay(fun (m, t) -> 
                            if t.Guid = m.SelectionId then baseMediumLow else transparent)
                        "DoubleClickCmd" |> Binding.cmd RmSelection
                        "TabToolTip" |> Binding.oneWay(fun (m, t) -> t.Address)
                    ]))
            "ViewBorder" |> Binding.subModelSeq(
                (fun m -> m.Tabs),
                (fun t -> t.Guid),
                (fun () -> [
                    "IsDisposed" |> Binding.oneWay(fun (m, t) -> t.IsDisposed)
                    "WvVisibility" |> Binding.oneWay(fun (m, t) -> 
                        if t.Guid = m.SelectionId then Visibility.Visible else Visibility.Hidden)
                    "Address" |> Binding.twoWay (
                        (fun (m, t) -> t.Address), 
                        (fun uri (m, t) -> SetAddress(t.Guid, uri)))
                    "NewWindowRequest" |> Binding.cmdParam(fun e m -> 
                        let args = e :?> RoutedEventArgs 
                        let wv = args.OriginalSource :?> FrameworkElement // actually it is webview2
                        AddTab (wv.Tag :?> string))
                    "GoForwardCache" |> Binding.oneWay(fun (m, t) -> t.GoForwardCache)
                    "GoBackCache" |> Binding.oneWay(fun (m, t) -> t.GoBackCache)
                    "ReloadCache" |> Binding.oneWay(fun (m, t) -> t.ReloadCache)
                ]))
        ]


let getDesignVm (m:'model, b:Binding<'model, 'msg> list) = ViewModel.designInstance m b
let main (window:Window) =
    let logger =
        LoggerConfiguration()
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Verbose)
            .WriteTo.Console()
            .CreateLogger()
    WpfProgram.mkProgram (fun () -> App.init (*window*)) App.update App.bindings
    |> WpfProgram.withLogger (new SerilogLoggerFactory(logger))
    |> WpfProgram.startElmishLoop window