# Simple DI
### Setup
First you need to create context for your scene.
To create context, create new script and inherit from class called DIContainer.

```csharp
import SimpleDI;
class Context : DIContainer {
}
```

Next you can start defining bindings and injecting the scene.

```csharp
import SimpleDI;
class Context : DIContainer {
    
    public ScriptableObjectDatabase Database; // project refrence
    
    void Start()
    {
        BindSingleton<IDatabase>(Database); // binding 
    
        InjectScene(); // inject scene
    }

}
```

The DiContainer set as -99999 in execution order, you can set manually
other execution order but remember it suppose to run first so the injection operation
happen first.
<br/><br/>
You can do the setup in any place as long as you know the execution order of your
program will stay valid.
<br/><br/>

### Binding methods

This package contains couple basic binding functions and they are: <br/>
Bind, BindResolver, BindFunction, BindSingleton and BindConstructor.
<br/><br/>

#### Bind
Resolves each type to a new instance.
```csharp
import SimpleDI;

interface IAd {
    void ShowAd(string id);
}

class GoodAd : IAd {
    public void ShowAd(string id)
    {
        // code here
    } 
}

class Context : DIContainer {
    void Start()
    {
        Bind<GoodAd>(); // binds concrete type
        Bind<IAd, GoodAd>(); // binds concrete type to interface/abstract class
    }
}
```
<br/>

#### BindResolver
Resolves with custom resolver.
```csharp
import SimpleDI;

interface IAd {
    void ShowAd(string id);
}

class GoodAd : IAd {
    public void ShowAd(string id)
    {
        // code here
    } 
}

class GoodResolver : IDIResolver {
    public object Resolve()
    {
        return new GoodAd();
    } 
}

class Context : DIContainer {
    void Start()
    {
        // type can be passed as generic or value.
        BindResolver(typeof(IAd), new GoodResolver());
        BindResolver<IAd>(new GoodResolver());
    }
}
```
<br/>

#### BindFunction
Resolves instance with function.
```csharp
import SimpleDI;

interface IAd {
    void ShowAd(string id);
}

class GoodAd : IAd {
    public void ShowAd(string id)
    {
        // code here
    } 
}

class Context : DIContainer {
    void Start()
    {
        // type can be passed as generic or value.
        BindFunction(typeof(IAd), GetIntance);
        BindFunction<IAd>(GetIntance);
    }
    
    object GetIntance()
    {
        return new GoodAd();
    }
}
```
<br/>

#### BindSingleton
Resolves singleton instance.
```csharp
import SimpleDI;

interface IAd {
    void ShowAd(string id);
}

class GoodAd : IAd {
    public void ShowAd(string id)
    {
        // code here
    } 
}

class Context : DIContainer {
    void Start()
    {
        // with given instance
        GoodAd goodAd = new GoodAd();
  
        // pass boolean as last paramater to indicate if instance 
        // need to be injected before use.
        BindSingleton(typeof(IAd), goodAd, false); 
        BindSingleton<IAd>(goodAd, false);
        
        // concrete instance
        BindSingleton<GoodAd>();
        
        // abstract type, concrete instance
        BindSingleton<IAd, GoodAd>();
    }
    
    object GetIntance()
    {
        return new GoodAd();
    }
}
```
<br/>

#### BindConstructor / Factory Function
Resolves each type new instance.
<br/>
NOTE: this binding doesnt work with IL2CPP only in c# backend.
<br/>
For now you can create custom factory resolver for IL2CPP.
```csharp
import SimpleDI;

class GoodAd {

    private float _delay;
    
    public GoodAd()
    {
        _delay = 0;
    }
    
    public GoodAd(float delay)
    {
        _delay = delay;
    }
}

class Context : DIContainer {
    void Start()
    {
        BindConstructor<GoodAd>(); // without paramanters
        BindConstructor<GoodAd, float>(); // with paramaters
    }
}
```
<br/>

### Injectees
This is how you define the injections in class.
```csharp
import SimpleDI;

interface IAd {
    void Show(string id);
}

interface IEneryManager {
    void AddEnergy(Energy energy);
}

interface IStats {
    int EnergyMultiplier {get;}
}

class Energy {

    public int Value {get => _stats.EnergyMultiplier * _energy}
    
    [Inject] private IStats _stats;
    private int _energy;
    
    public Energy(int energy)
    {
        _energy = energy;
    }
} 

class AdManager {
    
    // use attribute Inject to mark fields as injectables
    [Inject] private IAd _energyAd;
    private IEneryManager _energyManager;
    private Func<int, Energy> _newEnergy; // factory function
    
    // use attribute Inject to mark as injectable constructor
    [Inject]
    public AdManager(IEneryManager energyManager)
    {
        _energyManager = energyManager;
    }
    
    public void ShowEnergyAd()
    {
        _energyAd.Show(42069);
        _energyManager.AddEnergy(_newEnergy(9001));
    }
}
```
The ```Inject``` attribute on constructor used when creating new instance with ```NewInstanceResolver```
(any concrete type resolver except factory function).
<br/><br/>
Factory function treats any constructor as injectable, so no need to mark it.

### Instantiate

DIContainer class contains multiple function that called Instantiate.
Use this functions to create GameObjects with DI.

```csharp
using SimpleDI;
using UnityEngine;

class Spawner : ScriptableObject {

    public GameObject EnemyPrefab;

    [Inject] DIContainer _container;
    
    public void SpawnEnemy(Vector3 position)
    {
        var enemy = _container.Instantiate(EnemyPrefab);
        enemy.transform.position = position;
    }
}

```

### Scene/Project reference and Unity
You can use any reference because DIContainer is a MonoBehaviour.

```csharp

class SomeManager : MonoBehaviour {}

class Context : DIContainer {

    public SomeManager ManagerReference;

    void Start() {
        Bind<SomeManager>(ManagerReference);
    }
}
```
Be aware of the code execution order, you can still bind types and object
in runtime if you choose so. <br/>
The binding and injection happens in start of the scene when objects created
because we use the ```Start``` method, so again
<i><b>be aware of the execution order of the program</b></i>.

### Options
You can set two options in editor or in script:
```CreateConstructorBindingsWhileResolving``` this option will enable automatic
creation of Factory functions without the need to bind each one in the context.
<br/>
```CheckCircularDependenciesAtRuntime``` this option will enable runtime
circular dependency checker, highly recommend to turn on in development and
turn off in production (this is a very simple checker, you can leave it on 
if performance is not an issue).

```csharp
class Context : DIContainer {
    void Start()
    {
        CreateConstructorBindingsWhileResolving = true; // or false
        CheckCircularDependenciesAtRuntime = true; // or false
    }
}
```
<br/>

### Performance

We cache each injectee type by creating injector with Linq Expressions
and Reflection, so if you create a lot object of the same type the
performance could be near regular ```new object()``` call.


