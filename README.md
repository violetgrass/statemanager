# Violet StateManager

Violet.StateManager is a redux oriented general purpose .NET state manager. It tries to be pragmatic and as little opinionated as possible.

[![Nuget](https://img.shields.io/nuget/v/Violet.StateManager?style=flat-square)](https://www.nuget.org/packages/Violet.StateManager/)
![license:MIT](https://img.shields.io/github/license/violetgrass/statemanager?style=flat-square)
[![GitHub issues by-label](https://img.shields.io/github/issues/violetgrass/statemanager/bug?color=red&style=flat-square)](https://github.com/violetgrass/statemanager/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![GitHub issues by-label](https://img.shields.io/github/issues/violetgrass/statemanager/enhancement?color=green&style=flat-square)](https://github.com/violetgrass/statemanager/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)

[![CI Build](https://github.com/violetgrass/statemanager/actions/workflows/build-ci.yml/badge.svg)](https://github.com/violetgrass/statemanager/actions/workflows/build-ci.yml)
[![NuGet Release](https://github.com/violetgrass/statemanager/actions/workflows/build-release.yml/badge.svg)](https://github.com/violetgrass/statemanager/actions/workflows/build-release.yml)

## Features

- Reactive **State Manager** controlling stage changes using **Actions** (event definition), **Reducers** (how the state is changed) and **Effects** (side effects).
- API is **not opionated** and uses functions and inheritance free types. This allows your model to evolve freely without constraints by the state manager library.
- Management of **Sub States** allowing to define a more efficient management of reducers and simpler code in them.
- Simple **async/await** support throughout the whole API surface.
- Support for ILogger if needed.

## Example

Install NuGet package [`Violet.StateManager`](https://www.nuget.org/packages/Violet.StateManager)

Define an state (any class) and actions (any class)

````csharp
public record MyState(string A, string B);
public record DeleteA();
````

Setup your state manager and define how the state is changed

````csharp
using Violet.StateManager;

// define, initialize and setup your state to be managed
var state = new StateStore<MyState>(new MyState("a", "b"));

// register how state is changed when something happens (define a reducer)
state.Registration.On<DeleteA>(async (state, action) => state with { A = string.Empty });
````

Integrate into your app and trigger events ...

````csharp
// announce when something happens (dispatch an action)
var changedChange = await state.DispatchAsync(new DeleteA());

````

... and incorperate the changed state using the resulting change or observe it using `IObservable`.

````csharp
// use changedState from sample above 
if (changedState.A == string.Empty) { Console.WriteLine("Success, Library works"); }

// ... OR (registered before the dispatch) ...

using var disposable = state.Observable.Subscribe(state => /* do something with it */)
````

You can also integrate an effect to do side effects on state changes and dispatched actions

````csharp
state.Registration.OnChange(async (state, action) => /* do something with it */);
````

🚧 Asynchronous Action / Effects with return Actions is planned #1. Until then use the state manager directly to dispatch the result.

## Alternatives

- **Violet.StateManager**: this implementation
- [ReduxSimple](https://github.com/Odonno/ReduxSimple): One of the inspirations of this library, however focused on Reactive streams instead of async/await
- [Fluxor](https://github.com/mrpmorris/Fluxor) Object Oriented, Container based, Observable (state and actions)
- [Cortex.Net](https://github.com/jspuij/Cortex.Net): State Management and Observability Support
- ... many others ...

## Contributions

Everyone is welcome (private and commercial entities). Please read our Code of Conduct before participating in our project.

The product is licensed under MIT License to allow a easy and wide adoption into prviate and commercial products.