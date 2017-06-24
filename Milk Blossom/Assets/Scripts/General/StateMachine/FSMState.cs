using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class FSMState<T>
{
    // Abstract classes allow you to create classes that are incomplete
    // and must be completed in a derived class. They cannot be instantiated. 
    // They provide a common definition of a base class that multiple derived classes 
    // can share. 
    abstract public void Enter(T entity);
    abstract public void Execute(T entity);
    abstract public void Exit(T entity);
}
