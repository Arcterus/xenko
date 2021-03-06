// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An object that tracks the changes in the content of <see cref="IContentNode"/> referenced by a given root node.
    /// A <see cref="GraphNodeChangeListener"/> will raise events on changes on any node that is either a child, or the
    /// target of a reference from the root node, recursively.
    /// </summary>
    public class GraphNodeChangeListener : IDisposable
    {
        private readonly IContentNode rootNode;
        private readonly Func<MemberContent, IContentNode, bool> shouldRegisterNode;
        protected readonly HashSet<IContentNode> RegisteredNodes = new HashSet<IContentNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeChangeListener"/> class.
        /// </summary>
        /// <param name="rootNode">The root node for which to track referenced node changes.</param>
        /// <param name="shouldRegisterNode">A method that can indicate whether a node of the hierarchy should be registered to the listener.</param>
        public GraphNodeChangeListener(IContentNode rootNode, Func<MemberContent, IContentNode, bool> shouldRegisterNode = null)
        {
            this.rootNode = rootNode;
            this.shouldRegisterNode = shouldRegisterNode;
            RegisterAllNodes();
        }

        /// <summary>
        /// Raised before one of the node referenced by the related root node changes and before the <see cref="Changing"/> event is raised.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> PrepareChange;

        /// <summary>
        /// Raised after one of the node referenced by the related root node has changed and after the <see cref="Changed"/> event is raised.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> FinalizeChange;

        /// <summary>
        /// Raised before one of the node referenced by the related root node changes.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> Changing;

        /// <summary>
        /// Raised after one of the node referenced by the related root node has changed.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> Changed;

        /// <inheritdoc/>
        public void Dispose()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => UnregisterNode(node);
            visitor.Visit(rootNode);
        }

        protected virtual bool RegisterNode(IContentNode node)
        {
            // A node can be registered multiple times when it is referenced via multiple paths
            if (RegisteredNodes.Add(node))
            {
                node.PrepareChange += ContentPrepareChange;
                node.FinalizeChange += ContentFinalizeChange;
                node.Changing += ContentChanging;
                node.Changed += ContentChanged;
                return true;
            }
            return false;
        }

        protected virtual bool UnregisterNode(IContentNode node)
        {
            if (RegisteredNodes.Remove(node))
            {
                node.PrepareChange -= ContentPrepareChange;
                node.FinalizeChange -= ContentFinalizeChange;
                node.Changing -= ContentChanging;
                node.Changed -= ContentChanged;
                return true;
            }
            return false;
        }

        private void RegisterAllNodes()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => RegisterNode(node);
            visitor.ShouldVisit = shouldRegisterNode;
            visitor.Visit(rootNode);
        }

        private void ContentPrepareChange(object sender, ContentChangeEventArgs e)
        {
            var node = e.Content;
            if (node != null)
            {
                var visitor = new GraphVisitorBase();
                visitor.Visiting += (node1, path) => UnregisterNode(node1);
                visitor.ShouldVisit = shouldRegisterNode;
                switch (e.ChangeType)
                {
                    case ContentChangeType.ValueChange:
                        // The changed node itself is still valid, we don't want to unregister it
                        visitor.SkipRootNode = true;
                        visitor.Visit(node);
                        break;
                    case ContentChangeType.CollectionRemove:
                        if (node.IsReference && e.OldValue != null)
                        {
                            var removedNode = node.Reference.AsEnumerable[e.Index].TargetNode;
                            if (removedNode != null)
                            {
                                visitor.Visit(removedNode, node as MemberContent);
                            }
                        }
                        break;
                }
            }

            PrepareChange?.Invoke(sender, new GraphContentChangeEventArgs(e));
        }

        private void ContentFinalizeChange(object sender, ContentChangeEventArgs e)
        {
            var node = e.Content;
            if (node != null)
            {
                var visitor = new GraphVisitorBase();
                visitor.Visiting += (node1, path) => RegisterNode(node1);
                visitor.ShouldVisit = shouldRegisterNode;
                switch (e.ChangeType)
                {
                    case ContentChangeType.ValueChange:
                        // The changed node itself is still valid, we don't want to re-register it
                        visitor.SkipRootNode = true;
                        visitor.Visit(node);
                        break;
                    case ContentChangeType.CollectionAdd:
                        if (node.IsReference && e.NewValue != null)
                        {
                            IContentNode addedNode;
                            Index index;
                            if (!e.Index.IsEmpty)
                            {
                                index = e.Index;
                                addedNode = node.Reference.AsEnumerable[e.Index].TargetNode;
                            }
                            else
                            {
                                var reference = node.Reference.AsEnumerable.First(x => x.TargetNode.Retrieve() == e.NewValue);
                                index = reference.Index;
                                addedNode = reference.TargetNode;
                            }

                            if (addedNode != null)
                            {
                                var path = new GraphNodePath(node).PushIndex(index);
                                visitor.Visit(addedNode, node as MemberContent, path);
                            }
                        }
                        break;
                }
            }

            FinalizeChange?.Invoke(sender, new GraphContentChangeEventArgs(e));
        }

        private void ContentChanging(object sender, ContentChangeEventArgs e)
        {
            Changing?.Invoke(sender, new GraphContentChangeEventArgs(e));
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            Changed?.Invoke(sender, new GraphContentChangeEventArgs(e));
        }
    }
}
