using UnityEngine;
using System.Collections.Generic;
using ClipperLib;

namespace SimplySVG {
    public class CascadeContext {
        public GraphicalAttributes graphicalAttributes;
        public TransformAttributes transformAttributes;

        // Derived data
        public PolyTree clipStencil = null;

        public CascadeContext() {
            graphicalAttributes = GraphicalAttributes.CreateDefault();
            transformAttributes = TransformAttributes.CreateDefault();
        }

        /// <summary>
        /// Combines the current context with the target element context
        /// to form a new CascadeContext object. This new object does not
        /// reference stuff from the old CascadeContext or it's member objects,
        /// so it is safe to modify it.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public CascadeContext GatherElement(SVGElement element) {
            CascadeContext combinedContext = new CascadeContext();

            if (element is SVGStylable) {
                // Inherit the current GraphicalAttributes
                combinedContext.graphicalAttributes.Gather(graphicalAttributes);
                // Gather GraphicalAttributes from the next element
                combinedContext.graphicalAttributes.Gather(((SVGStylable)element).GetLocalAttributes());
            }

            if (element is SVGTransformable) {
                combinedContext.transformAttributes.Gather(transformAttributes);
                combinedContext.transformAttributes.Gather(((SVGTransformable)element).GetLocalTransformation());
            }

            GatherDerivedData(combinedContext, element);
            
            return combinedContext;
        }

        void GatherDerivedData(CascadeContext combinedContext, SVGElement element) {
            GatherClipStencil(combinedContext, element);
        }

        void GatherClipStencil(CascadeContext combinedContext, SVGElement element) {
            // Inherit the parent's clip path
            if (clipStencil != null) {
                combinedContext.clipStencil = clipStencil;
            }

            // Check if the element has a localy defined clip-path and combine it with the current clipStencil if it does
            if (element is SVGStylable) {
                SVGStylable stylableElement = (SVGStylable)element;
                GraphicalAttributes elementLocalGraphicals = stylableElement.GetLocalAttributes();

                if (elementLocalGraphicals.clipPath == null) {
                    // Nothing to do here. No updates are needed to the current clipStencil.
                    return;
                }

                SVGElement clipSVGElement = element.ownerDocument.GetElementById(elementLocalGraphicals.clipPath);
                if (clipSVGElement == null || !(clipSVGElement is ClipPathElement)) {
                    // The referenced clip-path is invalid
                    return;
                }

                ClipPathElement clipPathElement = (ClipPathElement)clipSVGElement;
                PolyTree localStencil = clipPathElement.GetStencil(combinedContext);
                
                if (localStencil == null) {
                    return;
                }

                if (combinedContext.clipStencil == null) {
                    // There is no parent clip stencil so the current local stencil is used as is
                    combinedContext.clipStencil = localStencil;

                } else {
                    // There is a parent stencil already present, so combine it with the current local stencil

                    PolyTree combined;
                    TriangulationUtility.ClipStencil(combinedContext.clipStencil, localStencil, out combined);

                    combinedContext.clipStencil = combined;
                }
            }
        }
    }
}
