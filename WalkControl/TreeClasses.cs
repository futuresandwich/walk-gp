﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace WalkControl
{
	public abstract class Node : ICloneable
	{
		public List<Node> Children { get; set; }
		public Node()
		{
			Children = new List<Node>();
		}

		public virtual void AddChild(Node n)
		{
			Children.Add(n);
		}

		public virtual void AddChild(Node n, int index)
		{
			Children.Insert(index, n);
		}

		public void RemoveChild(Node n)
		{
			Children.Remove(n);
		}

		#region ICloneable Members

		public abstract object Clone();

		#endregion

		public virtual void Mutate()
		{
		}

		public abstract string Serialize();
	}

	public class Conditional : Node
	{
		public Node Success { get; set; }
		public Node Failure { get; set; }
		public new List<Node> Children
		{
			get { return base.Children; }
			set { throw new NotImplementedException(); }
		}
		public virtual bool Evaluate(Dictionary<int, int> State) { return ConditionFunc.Invoke(State); }
		private Expression<Func<Dictionary<int, int>, bool>> condition;
		public Expression<Func<Dictionary<int, int>, bool>> Condition
		{
			get { return condition; }
			protected set { condition = value; ConditionFunc = condition.Compile(); }
		}
		public Func<Dictionary<int, int>, bool> ConditionFunc
		{
			get;
			protected set;
		}

		public Conditional(Expression<Func<Dictionary<int, int>, bool>> condition)
			: base()
		{
			Condition = condition;
		}


		public override object Clone()
		{
			var n = new Conditional(Condition);
			if (Success != null)
				n.Success = Success.Clone() as Node;
			if (Failure != null)
				n.Failure = Failure.Clone() as Node;
			return n;
		}

		/// <summary>
		/// Randomly add this node to success or failure trees
		/// </summary>
		/// <param name="n"></param>
		/// <param name="index"></param>
		public override void AddChild(Node n)
		{
			if (new Random((int)DateTime.Now.Ticks % Int32.MaxValue).Next(100) > 50)
			{
				if (Success == null)
					Success = n;
				else
					Success.AddChild(n);
			}
			else
			{
				if (Failure == null)
					Failure = n;
				else
					Failure.AddChild(n);
			}
		}

		/// <summary>
		/// Randomly add this node to success or failure trees
		/// </summary>
		/// <param name="n"></param>
		/// <param name="index"></param>
		public override void AddChild(Node n, int index)
		{
			AddChild(n);
		}

		public override string Serialize()
		{
			//not really relevent for condition
			return null;
		}
	}

	public class Action : Node
	{
		public Dictionary<int, int> Angles
		{
			get;
			protected set;
		}

		public Action(Dictionary<int, int> Angles)
			: base()
		{
			this.Angles = Angles;
		}

		public override object Clone()
		{
			var n = new Action(new Dictionary<int, int>(Angles));
			foreach (var c in Children)
			{
				n.AddChild(c.Clone() as Node);
			}
			return n;
		}

		public override string Serialize()
		{
			var s = new StringBuilder();
			foreach (var p in Angles)
			{
				if (s.Length > 0)
					s.Append(";");
				s.Append(p.Key);
				s.Append(":");
				s.Append(p.Value);
			}
			return s.ToString();
		}
	}
}
