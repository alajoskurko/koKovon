using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    private bool detectSwipes = false;
    private Vector2 fingerPositionDown;
    private Vector2 fingerPositionUp;
    [SerializeField]
    private bool detectSwipesOnlyAfterRelease = false;
    [SerializeField]
    private float minDistanceForSwipe = 40f;

    public static event Action<SwipeData> OnSwipe = delegate { };

    private void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                fingerPositionDown = touch.position;
                fingerPositionUp = touch.position;
            }
            if (!detectSwipesOnlyAfterRelease && touch.phase == TouchPhase.Moved)
            {
                fingerPositionDown = touch.position;
                DetectSwipe();
            }
            if (touch.phase == TouchPhase.Ended)
            {
                fingerPositionDown = touch.position;
                DetectSwipe();
            }
        }
    }

    private void DetectSwipe()
    {
        if (SwipeDistanceCheckMet())
        {
            if (IsVerticalSwipe())
            {
                var direction = fingerPositionDown.y - fingerPositionUp.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                SendSwipe(direction);
            }
            else
            {
                var direction = fingerPositionDown.x - fingerPositionUp.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                SendSwipe(direction);
            }
            fingerPositionUp = fingerPositionDown;
        }
    }

    private bool SwipeDistanceCheckMet()
    {
        return VerticalMovementDistance() > minDistanceForSwipe || HorizontalMovementDistance() > minDistanceForSwipe;
    }

    private float VerticalMovementDistance()
    {
        return Mathf.Abs(fingerPositionDown.y - fingerPositionUp.y);
    }

    private float HorizontalMovementDistance()
    {
        return Mathf.Abs(fingerPositionDown.x - fingerPositionUp.x);
    }

    private bool IsVerticalSwipe()
    {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }

    private void SendSwipe(SwipeDirection direction)
    {
        SwipeData swipeData = new SwipeData()
        {
            Direction = direction,
            StartPosition = fingerPositionDown,
            EndPosition = fingerPositionUp
        };
        OnSwipe(swipeData);
    }

    public struct SwipeData
    {
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public SwipeDirection Direction;
    }

    public enum SwipeDirection
    {
        Up, Down , Left, Right
    }
}
