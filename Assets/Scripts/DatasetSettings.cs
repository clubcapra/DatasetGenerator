using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Dataset", order = 1)]
public class DatasetSettings : ScriptableObject
{
    public DatasetClass[] classes;
}

[System.Serializable]
public class DatasetClass
{
    public Texture texture;
    public string name;
    public int objectClass;
}
