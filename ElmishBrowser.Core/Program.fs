module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF
open Elmish
open System.Windows

[<AutoOpen>]
module IList = 
    open System.Collections.Immutable
    let len = Seq.length
    let add d (x:ImmutableList<_>) = x.Add d 
    let rm ind (x:ImmutableList<_>) = x.RemoveAt ind 
    let init = ImmutableList.CreateRange

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
    open System.Collections.Immutable
    type Model =
        { 
            Tabs: Tab.TabItem ImmutableList
            SelectedId: int
        }
    
    type Msg =
        //| AddTab
        | Select of int
    
    let init =
        {
            Tabs = { 0 .. 5 } |> Seq.map(fun x -> { Content = $"Hello {x}" }) |> IList.init
            SelectedId = -1
        }, Cmd.none
    
    let update msg m = 
        match msg with 
        //| AddTab -> { m with Tabs = m.Tabs |> IList.add { Content = $"Hello {m.Tabs |> len}" } }, 
        //            Cmd.ofMsg (Select m.Tabs.Count)
        | Select i -> { m with SelectedId = i }, Cmd.none
    
    let bindings () : Binding<Model, Msg> list = 
        [
            //"AddTabCmd" |> Binding.cmd AddTab
            "TabSource" |> Binding.subModelSeq(
                (fun m -> m.Tabs |> Seq.indexed), 
                (fun (i, t) -> i), 
                (fun () ->  [
                        "TabContent" |> Binding.oneWay(fun (m, (i, t)) -> t.Content) 
                        "SelectItem" |> Binding.cmd(fun (m, (i, t)) -> Select i)
                        "TabBg" |> Binding.oneWay(fun (m, (i, t)) -> 
                            if i = m.SelectedId then accent else transparent)
                    ]))
            
            "ViewBorder" |> Binding.subModelSeq(
                (fun m -> m.Tabs |> Seq.indexed),
                (fun (i, v) -> i),
                (fun () -> [
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