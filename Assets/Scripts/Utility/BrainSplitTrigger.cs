using UnityEngine;

public class BrainSplitTrigger : MonoBehaviour
{
    [Header("Referencias")]
    public Transform leftHalf;        // Mitad izquierda del cerebro
    public Transform rightHalf;       // Mitad derecha del cerebro
    public Transform extraModel;      // Otro modelo que también debe subir

    [Header("Ajustes de movimiento")]
    public float separationDistance = 1f;   // Distancia de separación en Z
    public float brainLiftHeight = 0.5f;    // Altura de elevación de las mitades
    public float extraLiftHeight = 0.5f;    // Altura de elevación del modelo extra
    public float moveSpeed = 2f;            // Velocidad del movimiento

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;
    private Vector3 extraStartPos;

    private Vector3 leftTargetPos;
    private Vector3 rightTargetPos;
    private Vector3 extraTargetPos;

    private bool isPlayerNear = false;

    void Start()
    {
        // Guardamos posiciones iniciales
        leftStartPos = leftHalf.position;
        rightStartPos = rightHalf.position;
        if (extraModel != null)
            extraStartPos = extraModel.position;

        // Calculamos las posiciones de destino
        leftTargetPos = leftStartPos + new Vector3(0, brainLiftHeight, -separationDistance);
        rightTargetPos = rightStartPos + new Vector3(0, brainLiftHeight, separationDistance);
        if (extraModel != null)
            extraTargetPos = extraStartPos + new Vector3(0, extraLiftHeight, 0);
    }

    void Update()
    {
        if (isPlayerNear)
        {
            // Mover hacia posiciones separadas y elevadas
            leftHalf.position = Vector3.MoveTowards(leftHalf.position, leftTargetPos, moveSpeed * Time.deltaTime);
            rightHalf.position = Vector3.MoveTowards(rightHalf.position, rightTargetPos, moveSpeed * Time.deltaTime);

            if (extraModel != null)
                extraModel.position = Vector3.MoveTowards(extraModel.position, extraTargetPos, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Regresar a las posiciones originales
            leftHalf.position = Vector3.MoveTowards(leftHalf.position, leftStartPos, moveSpeed * Time.deltaTime);
            rightHalf.position = Vector3.MoveTowards(rightHalf.position, rightStartPos, moveSpeed * Time.deltaTime);

            if (extraModel != null)
                extraModel.position = Vector3.MoveTowards(extraModel.position, extraStartPos, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = false;
    }
}
