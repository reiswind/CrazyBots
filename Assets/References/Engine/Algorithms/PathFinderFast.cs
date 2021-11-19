
#define DEBUGONx

using System;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Engine.Master;
using Engine.Interface;

namespace Engine.Algorithms
{
    internal struct PathFinderNode
    {
        public int F;
        public int G;
        public int H;  // f = gone + heuristic
        public int X;
        public int Y;
        public int PX; // Parent
        public int PY;
    }
    internal enum PathFinderNodeType
    {
        Start = 1,
        End = 2,
        Open = 4,
        Close = 8,
        Current = 16,
        Path = 32
    }
    internal enum HeuristicFormula
    {
        Manhattan = 1,
        MaxDXDY = 2,
        DiagonalShortCut = 3,
        Euclidean = 4,
        EuclideanNoSQR = 5,
        Custom1 = 6,
        CubeDistance = 7
    }

    internal class PathFinderFast //: IPathFinder
    {
        #region Structs
	    [StructLayout(LayoutKind.Sequential, Pack=1)] 
        internal class PathFinderNodeFast
        {
            #region Variables Declaration
            public int     F; // f = gone + heuristic
            public int     G;
            public int  PX; // Parent
            public int  PY;
            public byte    Status;
            #endregion
        }
        #endregion

        #region Win32APIs
        //[System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint="RtlZeroMemory")]
        //public unsafe static extern bool ZeroMemory(byte* destination, int length);
        #endregion

        #region Events
        //public event PathFinderDebugHandler PathFinderDebug;
        #endregion


        // Heap variables are initializated to default, but I like to do it anyway
        //private byte[,]                         mGrid                   = null;
        private PriorityQueueB<Position2>             mOpen                   = null;
        private List<PathFinderNode>            mClose                  = new List<PathFinderNode>();
        private bool                            mStop                   = false;
        private bool                            mStopped                = true;
        private int                             mHoriz                  = 0;
        //private HeuristicFormula                mFormula                = HeuristicFormula.EuclideanNoSQR;
        private HeuristicFormula                mFormula                = HeuristicFormula.CubeDistance;
        
        //private bool                            mDiagonals              = true;
        private int                             mHEstimate              = 2;
        private bool                            mPunishChangeDirection  = false;
        private bool                            mTieBreaker             = false;
        //private bool                            mHeavyDiagonals         = false;
        private int                             mSearchLimit            = 2000;
        private double                          mCompletedTime          = 0;
        private bool                            mDebugProgress          = false;
        private bool                            mDebugFoundPath         = false;
        private Dictionary<Position2, PathFinderNodeFast> mCalcGrid;
        private byte                            mOpenNodeValue          = 1;
        private byte                            mCloseNodeValue         = 2;
        
        //Promoted local variables to member variables to avoid recreation between calls
        private int                             mH                      = 0;
        private Position2                        mLocation;
        private Position2                        mNewLocation;
        //private ushort                          mLocationX              = 0;
        //private ushort                          mLocationY              = 0;
        //private ushort                          mNewLocationX           = 0;
        //private ushort                          mNewLocationY           = 0;
        private int                             mCloseNodeCounter       = 0;
        //private ushort                          mGridX                  = 0;
        //private ushort                          mGridY                  = 0;
        //private ushort                          mGridXMinus1            = 0;
        //private ushort                          mGridYLog2              = 0;
        private bool                            mFound                  = false;
        //private sbyte[,]                        mDirection              = new sbyte[8,2]{{0,-1} , {1,0}, {0,1}, {-1,0}, {1,-1}, {1,1}, {-1,1}, {-1,-1}};
        private Position2                        mEndLocation;
        private int                             mNewG                   = 0;
        private Map map;
        public PathFinderFast(Map map)
        {
            this.map = map;
            //byte[,] grid = null;
            //if (grid == null)
            //    throw new Exception("Grid cannot be null");

            //mGrid           = grid;
            //mGridX          = (ushort) map.Model.MapWidth;
            //mGridY          = (ushort) map.Model.MapHeight;
            //mGridXMinus1    = (ushort) (mGridX - 1);
            //mGridYLog2      = (ushort) Math.Log(mGridY, 2);

            // This should be done at the constructor, for now we leave it here.
            /*
            if (Math.Log(mGridX, 2) != (int) Math.Log(mGridX, 2) ||
                Math.Log(mGridY, 2) != (int) Math.Log(mGridY, 2))
                throw new Exception("Invalid Grid, size in X and Y must be power of 2");
                */
            //if (mCalcGrid == null || mCalcGrid.Length != (mGridX * mGridY))
            //    mCalcGrid = new PathFinderNodeFast[mGridX * mGridY];
            mCalcGrid = new Dictionary<Position2, PathFinderNodeFast>();

            mOpen   = new PriorityQueueB<Position2>(new ComparePFNodeMatrix(mCalcGrid));
        }

        public bool Stopped
        {
            get { return mStopped; }
        }

        public HeuristicFormula Formula
        {
            get { return mFormula; }
            set { mFormula = value; }
        }

        /*public bool Diagonals
        {
            get { return mDiagonals; }
            set 
            { 
                mDiagonals = value; 
                if (mDiagonals)
                    mDirection = new sbyte[8,2]{{0,-1} , {1,0}, {0,1}, {-1,0}, {1,-1}, {1,1}, {-1,1}, {-1,-1}};
                else
                    mDirection = new sbyte[4,2]{{0,-1} , {1,0}, {0,1}, {-1,0}};
            }
        }*/

        /*public bool HeavyDiagonals
        {
            get { return mHeavyDiagonals; }
            set { mHeavyDiagonals = value; }
        }*/

        public int HeuristicEstimate
        {
            get { return mHEstimate; }
            set { mHEstimate = value; }
        }

        public bool PunishChangeDirection
        {
            get { return mPunishChangeDirection; }
            set { mPunishChangeDirection = value; }
        }

        public bool TieBreaker
        {
            get { return mTieBreaker; }
            set { mTieBreaker = value; }
        }

        public int SearchLimit
        {
            get { return mSearchLimit; }
            set { mSearchLimit = value; }
        }

        public double CompletedTime
        {
            get { return mCompletedTime; }
            set { mCompletedTime = value; }
        }

        public bool DebugProgress
        {
            get { return mDebugProgress; }
            set { mDebugProgress = value; }
        }

        public bool DebugFoundPath
        {
            get { return mDebugFoundPath; }
            set { mDebugFoundPath = value; }
        }
        public bool IgnoreVisibility { get; set;}


        #region Methods
        public void FindPathStop()
        {
            mStop = true;
        }

        public static int CalculatedPaths;

        public List<Position2> FindPath(Unit unit, Position2 start, Position2 end, bool ignoreIfToIsOccupied = false)
        {
            CalculatedPaths++;
            //lock(this)
            {
                //HighResolutionTime.Start();

                // Is faster if we don't clear the matrix, just assign different values for open and close and ignore the rest
                // I could have user Array.Clear() but using unsafe code is faster, no much but it is.
                //fixed (PathFinderNodeFast* pGrid = tmpGrid) 
                //    ZeroMemory((byte*) pGrid, sizeof(PathFinderNodeFast) * 1000000);

                mFound              = false;
                mStop               = false;
                mStopped            = false;
                mCloseNodeCounter   = 0;
                mOpenNodeValue      += 2;
                mCloseNodeValue     += 2;
                mOpen.Clear();
                mClose.Clear();

#if DEBUGON
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, start.X, start.Y, PathFinderNodeType.Start, -1, -1);
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, end.X, end.Y, PathFinderNodeType.End, -1, -1);
#endif

                mLocation = start; // (start.Y << mGridYLog2) + start.X;
                mEndLocation = end;  //(end.Y << mGridYLog2) + end.X;
                if (!mCalcGrid.ContainsKey(mLocation))
                    mCalcGrid.Add(mLocation, new PathFinderNodeFast());

                mCalcGrid[mLocation].G         = 0;
                mCalcGrid[mLocation].F         = mHEstimate;
                mCalcGrid[mLocation].PX = start.X; //.X;
                mCalcGrid[mLocation].PY = start.Y; //.Y;
                mCalcGrid[mLocation].Status    = mOpenNodeValue;

                int maxDepth = 1000;

                mOpen.Push(mLocation);
                while(mOpen.Count > 0 && !mStop)
                {
                    if (maxDepth-- < 0) 
                        return null;

                    mLocation = mOpen.Pop();

                    //Is it in closed list? means this node was already processed
                    if (mCalcGrid[mLocation].Status == mCloseNodeValue)
                        continue;

                    //mLocationX = (ushort)mLocation.X;// (mLocation & mGridXMinus1);
                    //mLocationY = (ushort)mLocation.Y; // (mLocation >> mGridYLog2);
                    
                    #if DEBUGON
                    if (mDebugProgress && PathFinderDebug != null)
                        PathFinderDebug(0, 0, mLocation & mGridXMinus1, mLocation >> mGridYLog2, PathFinderNodeType.Current, -1, -1);
                    #endif

                    if (mLocation == mEndLocation)
                    {
                        mCalcGrid[mLocation].Status = mCloseNodeValue;
                        mFound = true;
                        break;
                    }

                    if (mCloseNodeCounter > mSearchLimit)
                    {
                        mStopped = true;
                        //mCompletedTime = HighResolutionTime.GetTime();
                        return null;
                    }

                    if (mPunishChangeDirection)
                        mHoriz = (mLocation.X - mCalcGrid[mLocation].PX);

                    //Lets calculate each successors
                    Tile t = map.GetTile(mLocation);

                    //for (int i=0; i<(mDiagonals ? 8 : 4); i++)
                    foreach (Tile n in t.Neighbors)
                    {
                        if (!IgnoreVisibility)
                        {
                            //if (unit != null && !unit.Owner.VisiblePositions.Contains(n.Pos))
                            //    continue;
                        }
                        //mNewLocationX = (ushort)n.Pos.X;  //(ushort) (mLocationX + mDirection[i,0]);
                        //mNewLocationY = (ushort)n.Pos.Y; // (ushort) (mLocationY + mDirection[i,1]);
                        mNewLocation = n.Pos; //(mNewLocationY << mGridYLog2) + mNewLocationX;

                        //if (mNewLocationX >= mGridX || mNewLocationY >= mGridY)
                        //    continue;

                        if (unit != null)
                        {
                            Unit otherUnit = map.Units.GetUnitAt(mNewLocation);

                            if (otherUnit != null)
                            {
                                // Do not hit ourselfes
                                //if (otherUnit.Owner == unit.Owner)
                                if (mNewLocation != end)
                                    // Do not hit anyone but at end
                                    continue;
                            }
                        }

                        if (n.Pos == end && ignoreIfToIsOccupied)
                        {
                            // Ignore ende pos
                        }
                        else
                        {
                            if (!n.CanMoveTo(t)) // Include ende pos
                            {
                                continue;
                            }
                        }
                        // Unbreakeable?
                        //if (mGrid[mNewLocationX, mNewLocationY] == 0)
                        //    continue;

                        //if (mHeavyDiagonals && i>3)
                        //    mNewG = mCalcGrid[mLocation].G + (int) (mGrid[mNewLocationX, mNewLocationY] * 2.41);
                        //else
                        //    mNewG = mCalcGrid[mLocation].G + mGrid[mNewLocationX, mNewLocationY];

                        if (mPunishChangeDirection)
                        {
                            /*
                            if ((mNewLocation.X - mLocation.X) != 0)
                            {
                                if (mHoriz == 0)
                                    mNewG += Math.Abs(mNewLocation.X - end.X) + Math.Abs(mNewLocation.Y - end.Y);
                            }
                            if ((mNewLocation.Y - mLocation.Y) != 0)
                            {
                                if (mHoriz != 0)
                                    mNewG += Math.Abs(mNewLocation.X - end.X) + Math.Abs(mNewLocation.Y - end.Y);
                            }*/
                        }

                        //Is it open or closed?
                        if (!mCalcGrid.ContainsKey(mNewLocation))
                            mCalcGrid.Add(mNewLocation, new PathFinderNodeFast());
                        if (mCalcGrid[mNewLocation].Status == mOpenNodeValue || mCalcGrid[mNewLocation].Status == mCloseNodeValue)
                        {
                            // The current node has less code than the previous? then skip this node
                            if (mCalcGrid[mNewLocation].G <= mNewG)
                                continue;
                        }

                        mCalcGrid[mNewLocation].PX      = mLocation.X;
                        mCalcGrid[mNewLocation].PY      = mLocation.Y;
                        mCalcGrid[mNewLocation].G       = mNewG;

                        switch(mFormula)
                        {
                            default:
                            case HeuristicFormula.Manhattan:
                                //mH = mHEstimate * (Math.Abs(mNewLocation.X - end.X) + Math.Abs(mNewLocation.Y - end.Y));
                                break;
                            case HeuristicFormula.MaxDXDY:
                                //mH = mHEstimate * (Math.Max(Math.Abs(mNewLocation.X - end.X), Math.Abs(mNewLocation.Y - end.Y)));
                                break;
                            case HeuristicFormula.DiagonalShortCut:
                                //int h_diagonal  = Math.Min(Math.Abs(mNewLocation.X - end.X), Math.Abs(mNewLocation.Y - end.Y));
                                //int h_straight  = (Math.Abs(mNewLocation.X - end.X) + Math.Abs(mNewLocation.Y - end.Y));
                                //mH = (mHEstimate * 2) * h_diagonal + mHEstimate * (h_straight - 2 * h_diagonal);
                                break;
                            case HeuristicFormula.Euclidean:
                                //mH = (int) (mHEstimate * Math.Sqrt(Math.Pow((mNewLocation.Y - end.X) , 2) + Math.Pow((mNewLocation.Y - end.Y), 2)));
                                break;
                            case HeuristicFormula.EuclideanNoSQR:
                                //mH = (int) (mHEstimate * (Math.Pow((mNewLocation.X - end.X) , 2) + Math.Pow((mNewLocation.Y - end.Y), 2)));
                                break;
                            case HeuristicFormula.Custom1:
                                break;
                            case HeuristicFormula.CubeDistance:
                                mH = Position3.Distance(mNewLocation, end);
                                break;

                        }
                        if (mTieBreaker)
                        {
                            int dx1 = mLocation.X - end.X;
                            int dy1 = mLocation.Y - end.Y;
                            int dx2 = start.X - end.X;
                            int dy2 = start.Y - end.Y;
                            int cross = Math.Abs(dx1 * dy2 - dx2 * dy1);
                            mH = (int) (mH + cross * 0.001);
                        }
                        mCalcGrid[mNewLocation].F = mNewG + mH;

                        #if DEBUGON
                        if (mDebugProgress && PathFinderDebug != null)
                            PathFinderDebug(mLocationX, mLocationY, mNewLocationX, mNewLocationY, PathFinderNodeType.Open, mCalcGrid[mNewLocation].F, mCalcGrid[mNewLocation].G);
                        #endif

                        //It is faster if we leave the open node in the priority queue
                        //When it is removed, it will be already closed, it will be ignored automatically
                        //if (tmpGrid[newLocation].Status == 1)
                        //{
                        //    //int removeX   = newLocation & gridXMinus1;
                        //    //int removeY   = newLocation >> gridYLog2;
                        //    mOpen.RemoveLocation(newLocation);
                        //}

                        //if (tmpGrid[newLocation].Status != 1)
                        //{
                            mOpen.Push(mNewLocation);
                        //}
                        mCalcGrid[mNewLocation].Status = mOpenNodeValue;
                    }

                    mCloseNodeCounter++;
                    mCalcGrid[mLocation].Status = mCloseNodeValue;

                    #if DEBUGON
                    if (mDebugProgress && PathFinderDebug != null)
                        PathFinderDebug(0, 0, mLocationX, mLocationY, PathFinderNodeType.Close, mCalcGrid[mLocation].F, mCalcGrid[mLocation].G);
                    #endif
                }

                //mCompletedTime = HighResolutionTime.GetTime();
                if (mFound)
                {
                    mClose.Clear();
                    int posX = end.X;
                    int posY = end.Y;

                    //PathFinderNodeFast fNodeTmp = mCalcGrid[(end.Y << mGridYLog2) + end.X];
                    PathFinderNodeFast fNodeTmp = mCalcGrid[end];
                    PathFinderNode fNode;
                    fNode.F  = fNodeTmp.F;
                    fNode.G  = fNodeTmp.G;
                    fNode.H  = 0;
                    fNode.PX = fNodeTmp.PX;
                    fNode.PY = fNodeTmp.PY;
                    fNode.X  = end.X;
                    fNode.Y  = end.Y;

                    while(fNode.X != fNode.PX || fNode.Y != fNode.PY)
                    {
                        mClose.Add(fNode);
                        #if DEBUGON
                        if (mDebugFoundPath && PathFinderDebug != null)
                            PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
                        #endif
                        posX = fNode.PX;
                        posY = fNode.PY;
                        //fNodeTmp = mCalcGrid[(posY << mGridYLog2) + posX];
                        Position2 pos = new Position2(posX, posY);
                        if (!mCalcGrid.ContainsKey(pos))
                            mCalcGrid.Add(pos, new PathFinderNodeFast());
                        fNodeTmp = mCalcGrid[pos];
                        fNode.F  = fNodeTmp.F;
                        fNode.G  = fNodeTmp.G;
                        fNode.H  = 0;
                        fNode.PX = fNodeTmp.PX;
                        fNode.PY = fNodeTmp.PY;
                        fNode.X  = posX;
                        fNode.Y  = posY;
                    } 

                    mClose.Add(fNode);
                    #if DEBUGON
                    if (mDebugFoundPath && PathFinderDebug != null)
                        PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
                    #endif

                    mStopped = true;

                    List<Position2> route = new List<Position2>();
                    if (mClose != null && mClose.Count > 0)
                    {
                        Move move = new Move();
                        move.MoveType = MoveType.Move;
                        move.Positions = new List<Position2>();

                        for (int ndx = mClose.Count - 1; ndx >= 0; ndx--)
                        {
                            route.Add(new Position2(mClose[ndx].X, mClose[ndx].Y));
                        }
                    }
                    return route;
                }
                mStopped = true;
                return null;
            }
        }
        #endregion

        #region Inner Classes
        internal class ComparePFNodeMatrix : IComparer<Position2>
        {
            #region Variables Declaration
            Dictionary<Position2, PathFinderNodeFast> mMatrix;
            #endregion

            #region Constructors
            public ComparePFNodeMatrix(Dictionary<Position2, PathFinderNodeFast> matrix)
            {
                mMatrix = matrix;
            }
            #endregion

            #region IComparer Members
            public int Compare(Position2 a, Position2 b)
            {
                
                if (mMatrix[a].F > mMatrix[b].F)
                    return 1;
                else if (mMatrix[a].F < mMatrix[b].F)
                    return -1;
                return 0;
            }
            #endregion
        }
        #endregion
    }
}
