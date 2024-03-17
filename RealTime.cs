using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Unity.VisualScripting;
public class Realtime : MonoBehaviour
{
    // conexion con Firebase
    private FirebaseApp _app;
    // Singleton de la Base de Datos
    private FirebaseDatabase _db;
    // referencia a la 'coleccion' Clientes
    // ref a cosas de tu firebase
    private DatabaseReference _refClientes;
    // referencia a un cliente en concreto
    private DatabaseReference _refAA002;

    // -- PREFABS --
    // ref al cliente: prefabs del Firebase
    private DatabaseReference _refPrefabs;
    private DatabaseReference _refP1;

    // GameObject a modificar
    public GameObject ondavital;

    // GameObject a modificar
    public GameObject pickup1;

    // contador para updateS
    private float _i;
    
    /*
     * Base de datos usada en formato JSON
     *      {
              "Jugadores": {
                    "AA01": {
                      "nombre": "Vegeta",
                      "puntos": 0
                    },
                    "AA02": {
                      "nombre": "Son Goku",
                      "puntos": 1
                    }
               }
            }
     */
    
    // Start is called before the first frame update
    void Start()
    {
        // inicializamos contador
        _i = 0;
        
        // realizamos la conexion a Firebase
        _app = Conexion();
        
        // obtenemos el Singleton de la base de datos
        _db = FirebaseDatabase.DefaultInstance;
        
        // Obtenemos la referencia a TODA la base de datos
        // DatabaseReference reference = db.RootReference;
        
        // Definimos la referencia a Clientes
        _refClientes = _db.GetReference("Jugadores");
        
        // Definimos la referencia a AA02
        _refAA002 = _db.GetReference("Jugadores/AA02");

        // -- PREFABS --
        // ref prefabs
        _refPrefabs = _db.GetReference("prefabs");
        // ref al "prefabs" del firebase
        _refP1 = _db.GetReference("prefabs/p1");
        
        // -- PREFABS --
        // recogemos todos los valores de prefabs
        
        _refPrefabs.GetValueAsync().ContinueWithOnMainThread(task => {
            if(task.IsFaulted) {
                // handle the error...
            }
            else if(task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                // mostramos los datos
                RecorreResultado(snapshot);
                //Debug.Log(snapshot.value);
            }
        });
        
        
        // Recogemos todos los valores de Clientes
        _refClientes.GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    // Handle the error...
                }
                else if (task.IsCompleted) {
                    // task.Result --> datasnapshoot, captura lo queh hay arriba 
                    DataSnapshot snapshot = task.Result;
                    // mostramos los datos
                    RecorreResultado(snapshot);
                    //Debug.Log(snapshot.value);
                }
            });

        
        // Añadimos el evento cambia un valor
        _refAA002.ValueChanged += HandleValueChanged;

        // -- PREFABS --
        // añadimos el evnto cambia un valor
        _refP1.ValueChanged += HandleValueChanged_prefabs;

        // Añadimos un nodo
        AltaDevice();

        //Float x1 = snapshot.Children("prefabs").Children("p1").Children("x1").value // cojo valor.
    }

    
    // realizamos la conexion a Firebase
    // devolvemos una instancia de esta aplicacion
    FirebaseApp Conexion()
    {
        FirebaseApp firebaseApp = null;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                firebaseApp = FirebaseApp.DefaultInstance;
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
                firebaseApp = null;
            }
        });
            
        return firebaseApp;
    }
    
    // evento cambia valor en AA02
    // escalo objeto en la escena
    void HandleValueChanged(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Mostramos lo resultados
        MuestroJugador(args.Snapshot);

        // escalo objeto 
        float escala = float.Parse(args.Snapshot.Child("puntos").Value.ToString());
        Vector3 cambioEscala = new Vector3(escala, escala, escala);
        ondavital.transform.localScale = cambioEscala;

    }
    
    // -- PREFABS --
    // evento cambia valor en px
    // escalo objeto en la escena
    void HandleValueChanged_prefabs(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Mostramos lo resultados
        MuestroJugador(args.Snapshot); //?????

        // escalo objeto 
        float x = float.Parse(args.Snapshot.Child("x").Value.ToString());
        float y = float.Parse(args.Snapshot.Child("y").Value.ToString());
        float z = float.Parse(args.Snapshot.Child("z").Value.ToString());
        Vector3 cambioEscala = new Vector3(x, y, z);
        // cambio a position
        pickup1.transform.position = cambioEscala;

    }

    // recorro un snapshot de un nivel
    void RecorreResultado(DataSnapshot snapshot)
    {
        foreach(var resultado in snapshot.Children) // Clientes
        {
            Debug.LogFormat("Key = {0}", resultado.Key);  // "Key = AAxx"
            foreach(var levels in resultado.Children)
            {
                Debug.LogFormat("(key){0}:(value){1}", levels.Key, levels.Value);
            }
        }
    }
    
    // muestro un jugador
    void MuestroJugador(DataSnapshot jugador)
    {
        foreach (var resultado in jugador.Children) // jugador
        {
            Debug.LogFormat("{0}:{1}", resultado.Key, resultado.Value);
        }
    }


    // doy de alta un nodo con un identificador unico
    void AltaDevice()
    {
        // creo un nodo con un id unico del dispositivo que estoy creando, todas las app compartimos db pero segun
        // que dispo tendr un identificador unico.
        _refClientes.Child(SystemInfo.deviceUniqueIdentifier).Child("nombre").SetValueAsync("Mi dispositivo");

    }
    
    // Update is called once per frame
    void Update()
    {

        double playerx = ondavital.transform.position.x; // Obtenemos la posición x del objeto.
        double playery = ondavital.transform.position.y; // Obtenemos la posición y del objeto.
        
        // Actualizo la base de datos en cada frame, CUIDADO!!!!! 
        _refClientes.Child("AA01").Child("puntos").SetValueAsync(_i);
        
        _refPrefabs.Child("p1").Child("x").SetValueAsync(playerx+2);
        _refPrefabs.Child("p1").Child("y").SetValueAsync(playery+2);

        // -- PREFABS --
        //_refPrefabs.Child("p1").Child("x").SetValueAsync(_i);
        //_refPrefabs.Child("p1").Child("y").SetValueAsync(_i);
        //_refPrefabs.Child("p1").Child("z").SetValueAsync(_i);
        

        _i = _i + 0.01f;
    }
}
