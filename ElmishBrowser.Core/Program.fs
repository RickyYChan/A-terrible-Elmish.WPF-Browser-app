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
    let transparent = Color.Transparent |> box

[<AutoOpen>]
module Tab =
    type TabItem = 
        {
            Guid: Guid
            Content: obj 
            IsDisposed: bool 
        }
    let it = 
        {
            Guid = Guid.Empty
            Content = "Hello Elmish"
            IsDisposed = false
        }
    type TabMsg = Dispose 
    let tabUpdate msg m = 
        match msg with Dispose -> { m with IsDisposed = true }

module App = 

    type Model =
        { 
            Tabs: Tab.TabItem list
            SelectionId: Guid
        }
    
    type Msg =
        | None
        | AddTab
        | SelectOfGuid of Guid
        | RmSelection
        | RmAfterDisposing
    
    let init =
        {
            Tabs = []
            SelectionId = Guid.Empty
        }, Cmd.none
    
    let update msg m = 
        match msg with 
        | AddTab -> let id = idlist.genId()
                    { m with Tabs = m.Tabs @ [{ it with Content = $"Hello {m.Tabs |> len}" 
                                                        Guid = id }] }, 
                    Cmd.ofMsg (SelectOfGuid id)
        | SelectOfGuid i -> { m with SelectionId = i }, Cmd.none
        | RmSelection -> { m with Tabs = m.Tabs |> idlist.mapAtId m.SelectionId (tabUpdate Dispose) }, 
                            Cmd.ofMsg RmAfterDisposing
        | RmAfterDisposing -> { m with Tabs = m.Tabs |> idlist.rmAtId m.SelectionId }, 
                                Cmd.ofMsg (SelectOfGuid (m.Tabs |> idlist.getPreviousId m.SelectionId))
        | None -> m, Cmd.none
    let bindings () : Binding<Model, Msg> list = 
        [
            "AddTabCmd" |> Binding.cmd AddTab
            "RmTabCmd" |> Binding.cmd RmSelection
            "TabSource" |> Binding.subModelSeq(
                (fun m -> m.Tabs), 
                (fun t -> t.Guid), 
                (fun () ->  [
                        "TabContent" |> Binding.oneWay(fun (m, t) -> t.Content) 
                        "SelectItem" |> Binding.cmd(fun (m, t) -> SelectOfGuid t.Guid)
                        "TabBg" |> Binding.oneWay(fun (m, t) -> 
                            if t.Guid = m.SelectionId then accent else transparent)
                        "DoubleClickCmd" |> Binding.cmd RmSelection
                    ]))
            "ViewBorder" |> Binding.subModelSeq(
                (fun m -> m.Tabs),
                (fun t -> t.Guid),
                (fun () -> [
                    "IsDisposed" |> Binding.oneWay(fun (m, t) -> t.IsDisposed)
                    "WvVisibility" |> Binding.oneWay(fun (m, t) -> 
                        if t.Guid = m.SelectionId then Visibility.Visible else Visibility.Hidden)
                ]))
        ]


let getDesignVm (m:'model, b:Binding<'model, 'msg> list) = ViewModel.designInstance m b
let main window =
    let logger =
        LoggerConfiguration()
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Verbose)
            .WriteTo.Console()
            .CreateLogger()
    
    WpfProgram.mkProgram (fun () -> App.init) App.update App.bindings
    |> WpfProgram.withLogger (new SerilogLoggerFactory(logger))
    |> WpfProgram.startElmishLoop window