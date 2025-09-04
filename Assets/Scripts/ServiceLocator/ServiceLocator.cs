using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    private Dictionary<Type, object> services = new();

    private static ServiceLocator _instance;
    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ServiceLocator>();
                if (_instance == null)
                {
                    var newGO = new GameObject("ServiceLocator");
                    _instance = newGO.AddComponent<ServiceLocator>();
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetService<T>(T service)
    {
        var type = typeof(T);
        if (!services.ContainsKey(type))
        {
            services[type] = service;
        }
        else
        {
            services[type] = service;
        }
    }

    public T GetService<T>()
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
        {
            return (T)service;
        }

        return default;
    }
}
