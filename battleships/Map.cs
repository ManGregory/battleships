using System;
using System.Collections.Generic;
using System.Linq;

namespace battleships
{
	public enum CellState
	{
		Empty = 0,
		Ship,
		DeadOrWoundedShip,	
		Miss
	}

	public enum ShotResult
	{		
		Miss,		
		Wound,		
		Kill
	}

	public class Ship
	{
        public bool Alive { get { return AliveCells.Any(); } }

        public Vector Location { get; private set; }

        public int Size { get; private set; }

        ///<summary>Направление корабля. True — горизонтальное. False — вертикальное</summary>
        public bool Direction { get; private set; }

        public HashSet<Vector> AliveCells;

		public Ship(Vector location, int size, bool direction)
		{
			Location = location;
			Size = size;
			Direction = direction;
			AliveCells = new HashSet<Vector>(GetShipCells());
		}
		
		public List<Vector> GetShipCells()
		{
			var shipDirection = Direction ? new Vector(1, 0) : new Vector(0, 1);
			var shipCells = new List<Vector>();
			for (var i = 0; i < Size; i++)
			{
				var shipCell = shipDirection.Mult(i).Add(Location);
				shipCells.Add(shipCell);
			}
			return shipCells;
		}
	}

	public class Map
	{
		private static CellState[,] cells;
		public static Ship[,] shipsMap;

        public List<Ship> Ships = new List<Ship>();

        public int Width { get; private set; }

        public int Height { get; private set; }

	    public CellState this[Vector p]
        {
            get
            {
                return CheckBounds(p) ? cells[p.X, p.Y] : CellState.Empty; // Благодаря этому трюку иногда можно будет не проверять на выход за пределы поля. 
            }
            private set
            {
                if (!CheckBounds(p))
                    throw new IndexOutOfRangeException(p + " is not in the map borders"); 
                cells[p.X, p.Y] = value;
            }
        }

	    public Map(int width, int height)
		{
			Width = width;
			Height = height;
			cells = new CellState[width, height];
			shipsMap = new Ship[width, height];
		}

        private bool IsShipNeighborHoodEmpty(IEnumerable<Vector> shipCells)
        {
            return shipCells.SelectMany(CellNeighborhood).All(cell => this[cell] == CellState.Empty);
        }

        private bool IsShipFit(IEnumerable<Vector> shipCells)
        {
            return shipCells.All(CheckBounds);
        }

	    private bool CanSetShip(List<Vector> shipCells)
	    {	      
	        return
                IsShipNeighborHoodEmpty(shipCells) &&
                IsShipFit(shipCells);	        
	    }

	    ///<summary>Помещает корабль длиной size в точку location, смотрящий в направлении direction</summary>
		public bool SetShip(Vector location, int size, bool direction)
		{
			var ship = new Ship(location, size, direction);
			var shipCells = ship.GetShipCells();
			if (!CanSetShip(shipCells)) return false;
			foreach (var cell in shipCells)
			{
				this[cell] = CellState.Ship;
				shipsMap[cell.X, cell.Y] = ship;
			}
			Ships.Add(ship);
			return true;
		}

	    ///<summary>Получает результат выстрела по клетке target</summary>
		public ShotResult GetShotResult(Vector target)
		{
			var hit = CheckBounds(target) && this[target] == CellState.Ship;				
			if (hit)
			{
				var ship = shipsMap[target.X, target.Y];
				ship.AliveCells.Remove(target);
				this[target] = CellState.DeadOrWoundedShip;
				return ship.Alive ? ShotResult.Wound : ShotResult.Kill;
			}
			if (this[target] == CellState.Empty) this[target] = CellState.Miss;
			return ShotResult.Miss;
		}

		public IEnumerable<Vector> CellNeighborhood(Vector cell)
		{
			return
				from x in new[] {-1, 0, 1} 
				from y in new[] {-1, 0, 1} 
				let c = cell.Add(new Vector(x, y))
				where CheckBounds(c)
				select c;
		}

		public bool CheckBounds(Vector location)
		{
			return location.X >= 0 && location.X < Width && location.Y >= 0 && location.Y < Height;
		}
		
		public bool HasAliveShips()
		{
		    return Ships.Any(s => s.Alive);
		}
	}
}