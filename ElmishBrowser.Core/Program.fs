module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF
open Elmish
open System.Windows

[<AutoOpen>]
module IVec = 
    open System.Collections.Immutable
    type ivec<'a> = ImmutableList<'a>
    type ImmutableList with 
        static member add d (x:ivec<_>) = x.Add d 
        static member rmAt ind (x:ivec<_>) = x.RemoveAt ind 
        static member ofSeq x = ImmutableList.CreateRange x
    type ivec = ImmutableList

    let len = Seq.length

[<AutoOpen>]
/// use static classes: System.Windows.Media.Brushes and System.Windows.Application.    
/// can use mkProgramWithCmdMsg as an alternative.   
/// seems dirty.  
module FWColor = 
    type Color = System.Windows.Media.Brushes
    let accent = System.Windows.Application.Current.Resources.["ImmersiveSystemAccentBrush"]
    let transparent = Color.Transparent :> obj 


[<AutoOpen>]
module Tab =
    type TabItem = 
        {
            Content: obj 
        }

module App = 
    type Model =
        { 
            Tabs: Tab.TabItem ivec
            SelectedId: int
        }
    
    type Msg =
        | AddTab
        | RmSelection
        | Select of int
    
    let init =
        {
            Tabs = { 0 .. 5 } |> Seq.map(fun x -> { Content = $"Hello {x}" }) |> ivec.ofSeq
            SelectedId = -1
        }
        //, Cmd.none
        , Cmd.ofMsg (Select 0)
    
    let update msg m = 
        match msg with 
        | AddTab -> { m with Tabs = m.Tabs |> ivec.add { Content = $"Hello {m.Tabs |> len}" } }, 
                    Cmd.ofMsg (Select m.Tabs.Count)
        | Select i -> { m with SelectedId = i }, Cmd.none
        | RmSelection -> (if m.Tabs.Count = 0 then m else { m with Tabs = m.Tabs |> ivec.rmAt m.SelectedId }), 
                            Cmd.ofMsg (Select (m.Tabs.Count - 2)) // new-m - 1 ( new-m = old m - 1)
    
    let bindings () : Binding<Model, Msg> list = 
        [
            "AddTabCmd" |> Binding.cmd AddTab
            "RmTabCmd" |> Binding.cmd RmSelection
            "TabSource" |> Binding.subModelSeq(
                (fun m -> m.Tabs |> Seq.indexed), 
                (fun (i, t) -> i), 
                (fun () ->  [
                        "TabContent" |> Binding.oneWay(fun (m, (i, t)) -> t.Content) 
                        "SelectItem" |> Binding.cmd(fun (m, (i, t)) -> Select i)
                        "TabBg" |> Binding.oneWay(fun (m, (i, t)) -> 
                            if i = m.SelectedId then accent else transparent)
                        "DoubleClickCmd" |> Binding.cmd RmSelection
                    ]))
            
            "ViewBorder" |> Binding.subModelSeq(
                (fun m -> m.Tabs |> Seq.indexed),
                (fun (i, v) -> i),
                (fun () -> [
                    "DBGC" |> Binding.oneWay(fun _ -> System.Drawing.Color.Transparent)
                    "WvVisibility" |> Binding.oneWay(fun (m, (i, t)) -> 
                        if i = m.SelectedId then Visibility.Visible else Visibility.Hidden)
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