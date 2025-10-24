using UnityEngine;

public class InteractableInfo : MonoBehaviour
{
    [Header("Información del objeto")]
    public string nombreObjeto;
    [TextArea(2, 4)]
    public string descripcion;
    public Sprite imagen;
}
