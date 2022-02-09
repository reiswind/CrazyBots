using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    internal class UnitBullet
    {
        //Which angle do we need to hit the target?
        //This is a Quadratic equation so we will get 0, 1 or 2 answers
        //These answers are in this case angles
        //If we get 0 angles, it means we cant solve the equation, meaning that we can't hit the target because we are out of range
        //https://en.wikipedia.org/wiki/Projectile_motion
        public static void CalculateAngleToHitTarget(BulletData bulletData, Vector3 targetVec, out float? theta1, out float? theta2)
        {
            //Initial speed
            float v = bulletData.muzzleVelocity;

            //Vector3 targetVec = targetTrans.position - endOfBarrelTrans.position;

            //Vertical distance
            float y = targetVec.y;

            //Reset y so we can get the horizontal distance x
            targetVec.y = 0f;

            //Horizontal distance
            float x = targetVec.magnitude;

            //Gravity
            float g = 9.81f;


            //Calculate the angles
            float vSqr = v * v;

            float underTheRoot = (vSqr * vSqr) - g * (g * x * x + 2 * y * vSqr);

            //Check if we are within range
            if (underTheRoot >= 0f)
            {
                float rightSide = Mathf.Sqrt(underTheRoot);

                float top1 = vSqr + rightSide;
                float top2 = vSqr - rightSide;

                float bottom = g * x;

                theta1 = Mathf.Atan2(top1, bottom) * Mathf.Rad2Deg;
                theta2 = Mathf.Atan2(top2, bottom) * Mathf.Rad2Deg;
            }
            else
            {
                theta1 = null;
                theta2 = null;
            }
        }

        public static void DrawTrajectoryPath(LineRenderer lineRenderer, Vector3 currentVel, Vector3 currentPos)
        {
            //Start values
            //Vector3 currentVel = barrelTrans.forward * bulletData.muzzleVelocity;
            //Vector3 currentPos = endOfBarrelTrans.position;

            Vector3 newPos = Vector3.zero;
            Vector3 newVel = Vector3.zero;

            List<Vector3> bulletPositions = new List<Vector3>();

            //Build the trajectory line
            bulletPositions.Add(currentPos);

            //I prefer to use a maxIterations instead of a while loop 
            //so we always avoid stuck in infinite loop and have to restart Unity
            //You might have to change this value depending on your values
            int maxIterations = 10000;
            float timeStep;
            timeStep = Time.fixedDeltaTime * 1f;

            for (int i = 0; i < maxIterations; i++)
            {
                //Calculate the bullets new position and new velocity
                CurrentIntegrationMethod(timeStep, currentPos, currentVel, out newPos, out newVel);

                //Set the new value to the current values
                currentPos = newPos;
                currentVel = newVel;

                //Add the new position to the list with all positions
                bulletPositions.Add(currentPos);

                //The bullet has hit the ground because we assume 0 is ground height
                //This assumes the bullet is fired from a position above 0 or the loop will stop immediately
                if (currentPos.y < 0f)
                {
                    break;
                }

                //A warning message that something might be wrong
                if (i == maxIterations - 1)
                {
                    Debug.Log("The bullet newer hit anything because we reached max iterations");
                }
            }

            //Display the bullet positions with a line renderer
            lineRenderer.positionCount = bulletPositions.Count;
            lineRenderer.SetPositions(bulletPositions.ToArray());
        }



        //Choose which integration method you want to use to simulate how the bullet fly
        public static void CurrentIntegrationMethod(float timeStep, Vector3 currentPos, Vector3 currentVel, out Vector3 newPos, out Vector3 newVel)
        {
            //IntegrationMethods.BackwardEuler(timeStep, currentPos, currentVel, out newPos, out newVel);

            IntegrationMethods.ForwardEuler(timeStep, currentPos, currentVel, out newPos, out newVel);

            //IntegrationMethods.Heuns(timeStep, currentPos, currentVel, out newPos, out newVel);

            //IntegrationMethods.HeunsNoExternalForces(timeStep, currentPos, currentVel, out newPos, out newVel);
        }
    }

    public static class IntegrationMethods
    {
        private static Vector3 gravityVec = new Vector3(0f, -9.81f, 0f);



        //Integration method 1
        public static void BackwardEuler(float timeStep, Vector3 currentPos, Vector3 currentVel, out Vector3 newPos, out Vector3 newVel)
        {
            //Add all factors that affects the acceleration
            //Gravity
            Vector3 accFactor = gravityVec;


            //Calculate the new velocity and position
            //y_k+1 = y_k + timeStep * f(t_k+1, y_k+1)

            //This assumes the acceleration is the same next time step
            newVel = currentVel + timeStep * accFactor;

            newPos = currentPos + timeStep * newVel;
        }



        //Integration method 2
        public static void ForwardEuler(float timeStep, Vector3 currentPos, Vector3 currentVel, out Vector3 newPos, out Vector3 newVel)
        {
            //Add all factors that affects the acceleration
            //Gravity
            Vector3 accFactor = gravityVec;


            //Calculate the new velocity and position
            //y_k+1 = y_k + timeStep * f(t_k, y_k)

            newVel = currentVel + timeStep * accFactor;

            newPos = currentPos + timeStep * currentVel;
        }



        //Integration method 3
        //upVec is a vector perpendicular (in the upwards direction) to the direction the bullet is travelling in
        //is only needed if we calculate the lift force
        public static void Heuns(float timeStep, Vector3 currentPos, Vector3 currentVel, Vector3 upVec, BulletData bulletData, out Vector3 newPos, out Vector3 newVel)
        {
            //Add all factors that affects the acceleration
            //Gravity
            Vector3 accFactorEuler = gravityVec;
            //Drag
            accFactorEuler += BulletPhysics.CalculateBulletDragAcc(currentVel, bulletData);
            //Lift 
            accFactorEuler += BulletPhysics.CalculateBulletLiftAcc(currentVel, bulletData, upVec);


            //Calculate the new velocity and position
            //y_k+1 = y_k + timeStep * 0.5 * (f(t_k, y_k) + f(t_k+1, y_k+1))
            //Where f(t_k+1, y_k+1) is calculated with Forward Euler: y_k+1 = y_k + timeStep * f(t_k, y_k)

            //Step 1. Find new pos and new vel with Forward Euler
            Vector3 newVelEuler = currentVel + timeStep * accFactorEuler;

            //New position with Forward Euler (is not needed here)
            //Vector3 newPosEuler = currentPos + timeStep * currentVel;


            //Step 2. Heuns method's final step
            //If we take drag into account, then acceleration is not constant - it also depends on the velocity
            //So we have to calculate another acceleration factor
            //Gravity
            Vector3 accFactorHeuns = gravityVec;
            //Drag
            //This assumes that windspeed is constant between the steps, which it should be because wind doesnt change that often
            accFactorHeuns += BulletPhysics.CalculateBulletDragAcc(newVelEuler, bulletData);
            //Lift 
            accFactorHeuns += BulletPhysics.CalculateBulletLiftAcc(newVelEuler, bulletData, upVec);

            newVel = currentVel + timeStep * 0.5f * (accFactorEuler + accFactorHeuns);

            newPos = currentPos + timeStep * 0.5f * (currentVel + newVelEuler);
        }



        //Integration method 3.1
        //No external bullet forces except gravity
        //Makes it easier to see if the external forces are working if we display this trajectory
        public static void HeunsNoExternalForces(float timeStep, Vector3 currentPos, Vector3 currentVel, out Vector3 newPos, out Vector3 newVel)
        {
            //Add all factors that affects the acceleration
            //Gravity
            Vector3 accFactor = gravityVec;


            //Calculate the new velocity and position
            //y_k+1 = y_k + timeStep * 0.5 * (f(t_k, y_k) + f(t_k+1, y_k+1))
            //Where f(t_k+1, y_k+1) is calculated with Forward Euler: y_k+1 = y_k + timeStep * f(t_k, y_k)

            //Step 1. Find new pos and new vel with Forward Euler
            Vector3 newVelEuler = currentVel + timeStep * accFactor;

            //New position with Forward Euler (is not needed)
            //Vector3 newPosEuler = currentPos + timeStep * currentVel;

            //Step 2. Heuns method's final step if acceleration is constant
            newVel = currentVel + timeStep * 0.5f * (accFactor + accFactor);

            newPos = currentPos + timeStep * 0.5f * (currentVel + newVelEuler);
        }
    }


    public class BulletData //: MonoBehaviour
    {
        //Data belonging to this bullet type
        //The initial speed [m/s]
        public float muzzleVelocity = 10.5f;
        //Mass [kg]
        public float m = 50.2f;
        //Radius [m]
        public float r = 0.05f;
        //Coefficients, which is a value you can't calculate - you have to simulate it in a wind tunnel
        //and they also depends on the speed, so we pick some average value
        //Drag coefficient (Tesla Model S has the drag coefficient 0.24)
        public float C_d = 0.5f;
        //Lift coefficient
        public float C_l = 0.5f;


        //External data (shouldn't maybe be here but is easier in this tutorial)
        //Wind speed [m/s]
        public Vector3 windSpeedVector = new Vector3(0f, 0f, 0f);
        //The density of the medium the bullet is travelling in, which in this case is air at 15 degrees [kg/m^3]
        public float rho = 1.225f;
    }
    public static class BulletPhysics
    {
        //Calculate the bullet's drag acceleration
        public static Vector3 CalculateBulletDragAcc(Vector3 bulletVel, BulletData bulletData)
        {
            //If you have a wind speed in your game, you can take that into account here:
            //https://www.youtube.com/watch?v=lGg7wNf1w-k
            Vector3 bulletVelRelativeToWindVel = bulletVel - bulletData.windSpeedVector;

            //Step 1. Calculate the bullet's drag force [N]
            //https://en.wikipedia.org/wiki/Drag_equation
            //F_drag = 0.5 * rho * v^2 * C_d * A 

            //The velocity of the bullet [m/s]
            float v = bulletVelRelativeToWindVel.magnitude;
            //The bullet's cross section area [m^2]
            float A = Mathf.PI * bulletData.r * bulletData.r;

            float dragForce = 0.5f * bulletData.rho * v * v * bulletData.C_d * A;


            //Step 2. We need to add an acceleration, not a force, in the integration method [m/s^2]
            //Drag acceleration F = m * a -> a = F / m
            float dragAcc = dragForce / bulletData.m;

            //SHould be in a direction opposite of the bullet's velocity vector
            Vector3 dragVec = dragAcc * bulletVelRelativeToWindVel.normalized * -1f;


            return dragVec;
        }



        //Calculate the bullet's lift acceleration
        public static Vector3 CalculateBulletLiftAcc(Vector3 bulletVel, BulletData bulletData, Vector3 bulletUpDir)
        {
            //If you have a wind speed in your game, you can take that into account here:
            //https://www.youtube.com/watch?v=lGg7wNf1w-k
            Vector3 bulletVelRelativeToWindVel = bulletVel - bulletData.windSpeedVector;

            //Step 1. Calculate the bullet's lift force [N]
            //https://en.wikipedia.org/wiki/Lift_(force)
            //F_lift = 0.5 * rho * v^2 * S * C_l 

            //The velocity of the bullet [m/s]
            float v = bulletVelRelativeToWindVel.magnitude;
            //Planform (projected) wing area, which is assumed to be the same as the cross section area [m^2]
            float S = Mathf.PI * bulletData.r * bulletData.r;

            float liftForce = 0.5f * bulletData.rho * v * v * S * bulletData.C_l;

            //Step 2. We need to add an acceleration, not a force, in the integration method [m/s^2]
            //Drag acceleration F = m * a -> a = F / m
            float liftAcc = liftForce / bulletData.m;

            //The lift force acts in the up-direction = perpendicular to the velocity direction it travels in
            Vector3 liftVec = liftAcc * bulletUpDir;


            return liftVec;
        }
    }

}
