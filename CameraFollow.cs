using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    public Controller2D target;
    public Player player;
    public float verticalOffset;
    public float lookAheadDstX;
    public float lookSmoothTimeX;
    public float verticalSmoothTime;
    public Vector2 focusAreaSize;

    FocusArea focusArea;

    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirX;
    float smoothLookVelocityX;
    float smoothVelocityY;
    [SerializeField] float rightWallX;
    [SerializeField] float leftWallX;
    [SerializeField] float wallOffset;

    float newVerticalOffset;

    bool lookAheadStopped;

    void Start()
    {
        focusArea = new FocusArea(target.collider.bounds, focusAreaSize);
        newVerticalOffset = verticalOffset;
    }

    void LateUpdate()
    {
        focusArea.Update(target.collider.bounds);

        //Debug.Log(player.GetDirectionalInput().y);
        if (player.GetDirectionalInput().y < 0)
        {
            
            newVerticalOffset = Mathf.Clamp(newVerticalOffset - 100f * Time.deltaTime, -verticalOffset * 2, verticalOffset);
        }
        else if(player.GetDirectionalInput().y > 0) // Look up button conflicted with w platform
        {
            // newVerticalOffset = Mathf.Clamp(newVerticalOffset + 100f * Time.deltaTime, -verticalOffset, verticalOffset * 2);
        }
        else
        {
            newVerticalOffset = verticalOffset;
        }

        Vector2 focusPosition = focusArea.centre + Vector2.up * newVerticalOffset; // change vertical offset to newVerticalOffset to use above logic

        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
            if (Mathf.Sign(target.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && target.playerInput.x != 0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDstX;
            }
            else
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
                }
            }
        }


        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
        focusPosition.x = Mathf.Clamp(currentLookAheadX + focusPosition.x, leftWallX - wallOffset, rightWallX + wallOffset);
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube(focusArea.centre, focusAreaSize);
    }

    struct FocusArea
    {
        public Vector2 centre;
        public Vector2 velocity;
        float left, right;
        float top, bottom;


        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;

            velocity = Vector2.zero;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        public void Update(Bounds targetBounds)
        {
            float shiftX = 0;
            if (targetBounds.min.x < left)
            {
                shiftX = targetBounds.min.x - left;
            }
            else if (targetBounds.max.x > right)
            {
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.min.y < bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }
            top += shiftY;
            bottom += shiftY;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }

}