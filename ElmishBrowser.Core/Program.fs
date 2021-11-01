module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF
open Elmish
open System.Windows

[<AutoOpen>]
module Seq = 
    let len = Seq.length
    module Seq =
        let rmAt ind s = s |> Seq.indexed |> Seq.filter(fun (i, e) -> i <> ind) |> Seq.map snd // filteri
        let addLast s m = seq { yield! m; yield s }
        let mapAt ind f = Seq.mapi(fun i a -> if i = ind then f a else a)

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
            Guid: int
            Content: obj 
            IsDisposed: bool 
        }
    let it = 
        {
            Guid = -1 
            Content = "Hello Elmish"
            IsDisposed = false
        }
    type TabMsg = Dispose 
    let tabUpdate msg m = 
        match msg with Dispose -> { m with IsDisposed = true }

module App = 

    type Model =
        { 
            Tabs: Tab.TabItem seq
            SelectionId: int
        }
    
    type Msg =
        | None
        | AddTab
        | Select of int
        | RmSelection
        | RmAfterDisposing
        | MapId
    
    let init =
        {
            Tabs = { 0 .. 5 } |> Seq.map(fun x -> { it with Content = $"Hello {x}"; Guid = x })
            SelectionId = -1
        }
        //, Cmd.none
        , Cmd.ofMsg (Select 0)
    
    let update msg m = 
        match msg with 
        | AddTab -> { m with Tabs = m.Tabs |> Seq.addLast { it with Content = $"Hello {m.Tabs |> len}" 
                                                                    Guid = m.Tabs |> len }
                             SelectionId = m.Tabs |> len }, 
                    Cmd.none
        | Select i -> { m with SelectionId = i }, Cmd.none
        | RmSelection -> { m with Tabs = m.Tabs |> Seq.mapAt m.SelectionId (tabUpdate Dispose) }, 
                            Cmd.ofMsg RmAfterDisposing
        | RmAfterDisposing -> { m with Tabs = m.Tabs |> Seq.rmAt m.SelectionId }, 
                                Cmd.batch [
                                    Cmd.ofMsg MapId 
                                    Cmd.ofMsg (Select (len m.Tabs - 2) )
                                ]
        | MapId -> { m with Tabs = m.Tabs |> Seq.mapi(fun i t -> { t with Guid = i }) }, Cmd.none
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
                        "SelectItem" |> Binding.cmd(fun (m, t) -> Select t.Guid)
                        "TabBg" |> Binding.oneWay(fun (m, t) -> 
                            if t.Guid = m.SelectionId then accent else transparent)
                        //"DoubleClickCmd" |> Binding.cmd Dispose
                    ]))
            //"TabSource" |> Binding.subModelSeq(
            //    (fun () ->  [
            //            "TabContent" |> Binding.oneWay(fun t -> t.Content) 
            //            "SelectItem" |> Binding.cmd(fun t -> Select t.Guid)
            //            "TabBg" |> Binding.oneWay(fun t -> 
            //                if t.Guid = m.SelectionId then accent else transparent) // how to use the "m" here?
            //        ]))
            //    |> Binding.mapModel(fun m -> m.Tabs)
            //    |> Binding.mapMsg(fun (i, msg) -> msg)
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