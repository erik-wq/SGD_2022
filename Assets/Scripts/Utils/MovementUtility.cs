using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementUtility
{
    public static Vector2 CastAndAdjust(Rigidbody2D rigidBody, Vector2 direction, ContactFilter2D filter, List<RaycastHit2D> collisions, float distance)
    {
        Physics2D.queriesHitTriggers = false;
        int collisionsCount = rigidBody.Cast(direction,
                    filter,
                    collisions,
                    distance
                  );

        if (collisionsCount == 0)
        {
            Physics2D.queriesHitTriggers = true;
            return direction;
        }
        else if (collisions[0].transform.tag != "Player")
        {
            var xDirection = direction;
            xDirection.Set(direction.x, 0);
            if (xDirection != Vector2.zero)
            {
                collisionsCount = rigidBody.Cast(xDirection,
                                               filter,
                                               collisions,
                                               distance);
                if (collisionsCount == 0)
                {
                    Physics2D.queriesHitTriggers = true;
                    if (xDirection.x > 0)
                        return new Vector2(1, 0);
                    return new Vector2(-1, 0);
                }
            }

            direction.Set(0, direction.y);
            if (direction != Vector2.zero)
            {
                collisionsCount = rigidBody.Cast(direction,
                                   filter,
                                   collisions,
                                   distance);

                if (collisionsCount == 0)
                {
                    Physics2D.queriesHitTriggers = true;
                    if (direction.y > 0)
                        return new Vector2(0, 1);
                    return new Vector2(0, -1);
                }
            }
        }

        Physics2D.queriesHitTriggers = true;
        return Vector2.zero;
    }

}
