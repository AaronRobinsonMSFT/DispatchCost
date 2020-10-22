# Dispatch to managed code

1) Load [`DispatchCost.sln`](./DispatchCost.sln) in Visual Studio.
1) Set the configuration and platform to `Release|x64`
    - There is a hardcoded path in [`App\Program.cs`](./App/Program.cs) to a native binary in the solution. If the above configuration/platform is not selected there will be runtime errors.
1) Build and launch the console application.

The performance of the approaches can be viewed more precisely using the Visual Studio Performance Profiler (Ctrl+F2). Found under the Debug menu item.