module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF
module Tab = begin
    type TabItem = 
        {
            Content: obj 
        }
    type Msg = None
    let init = { Content = "Hello world!" }
    let update msg m = m 
    //let bindings () : Binding<TabItem, Msg> list = 
    //    [
    //        "TabContent" |> Binding.oneWay (fun m -> m.Content)
    //    ]
    end 
module App = begin
    type Model =
        { 
            Tabs: Tab.TabItem list 
        }
    
    type Msg =
        | AddTab
    
    let init =
        {
            Tabs = [ ] |> List.mapi(fun i x -> { Content = $"Hello {x}" })
        }
    
    let update msg m = 
        match msg with 
        | AddTab -> { m with Tabs = { Content = $"Hello {m.Tabs.Length}" } :: m.Tabs }
    
    let bindings () : Binding<Model, Msg> list = 
        [
            "AddTabCmd" |> Binding.cmd AddTab
            "TabSource" |> Binding.subModelSeq(
                (fun m -> m.Tabs |> List.indexed), 
                (fun (i, t) -> i), 
                (fun () -> 
                    [
                        "TabContent" |> Binding.oneWay(fun (m, (i, t)) -> t.Content)   
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