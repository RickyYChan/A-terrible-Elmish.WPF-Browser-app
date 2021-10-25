module ElmishBrowser.Core.Program

open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF

type Model =
    { 
        None: int
    }

type Msg =
    | None

let init =
    {
        None = 0 
    }

let update msg m = 
    match msg with 
    | None -> m

let bindings () : Binding<Model, Msg> list = 
    [
        
    ]

let getDesignVm (m:'T, b:Binding<_, _> list) = ViewModel.designInstance m b

let main window =
    let logger =
        LoggerConfiguration()
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Verbose)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Verbose)
            .WriteTo.Console()
            .CreateLogger()
    
    WpfProgram.mkSimple (fun () -> init) update bindings
    |> WpfProgram.withLogger (new SerilogLoggerFactory(logger))
    |> WpfProgram.startElmishLoop window