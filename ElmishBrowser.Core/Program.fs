module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF

[<AutoOpen>]
/// use static classes: System.Windows.Media.Brushes and System.Windows.Application.    
/// can use mkProgramWithCmdMsg as an alternative.   
/// seems dirty.  
module FWColor = begin 
    type Color = System.Windows.Media.Brushes
    let accent = System.Windows.Application.Current.Resources.["ImmersiveSystemAccentBrush"]
    let transparent = Color.Transparent :> obj 
    end 

module Tab = begin
    type TabItem = 
        {
            Content: obj 
        }
    end 
module App = begin
    type Model =
        { 
            Tabs: Tab.TabItem list 
            SelectedId: int option
        }
    
    type Msg =
        | AddTab
        | Select of int option
    
    let init =
        {
            Tabs = [ ] |> List.mapi(fun i x -> { Content = $"Hello {x}" })
            SelectedId = None
        }
    
    let update msg m = 
        match msg with 
        | AddTab -> { m with Tabs = { Content = $"Hello {m.Tabs.Length}" } :: m.Tabs }
        | Select i -> { m with SelectedId = i }
    
    let bindings () : Binding<Model, Msg> list = 
        [
            "AddTabCmd" |> Binding.cmd AddTab
            "TabSource" |> Binding.subModelSeq(
                (fun m -> m.Tabs |> List.indexed), 
                (fun (i, t) -> i), 
                (fun () -> 
                    [
                        "TabContent" |> Binding.oneWay(fun (m, (i, t)) -> t.Content) 
                        "SelectItem" |> Binding.cmd(fun (m, (i, t)) -> Select (Some i))
                        "TabBg" |> Binding.oneWay(fun (m, (i, t)) -> 
                            if Some i = m.SelectedId then accent else transparent)
                    ])
            )
        ]
    end 

let getDesignVm (m:'T, b:Binding<_, _> list) = ViewModel.designInstance m b
let main window =
    let logger =
        LoggerConfiguration()
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Verbose)
            .WriteTo.Console()
            .CreateLogger()
    
    WpfProgram.mkSimple (fun () -> App.init) App.update App.bindings
    |> WpfProgram.withLogger (new SerilogLoggerFactory(logger))
    |> WpfProgram.startElmishLoop window