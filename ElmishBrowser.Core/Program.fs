module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF
module Tab = begin
    type TabItem = 
        {
            Guid: int
            Content: obj 
        }
    type Msg = None
    let init = { Guid = 0; Content = "Hello world!" }
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
        | None
    
    let init =
        {
            Tabs = [ 0 .. 5 ] |> List.mapi(fun i x -> { Guid = i; Content = $"Hello {x}" })
        }
    
    let update msg m = 
        match msg with 
        | None -> m 
    
    let bindings () : Binding<Model, Msg> list = 
        [

            "TabSource" |> Binding.subModelSeq(
                (fun m -> m.Tabs), 
                (fun t -> t.Guid), 
                (fun () -> 
                    [
                        "TabContent" |> Binding.oneWay(fun (m, t) -> t.Content)   
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