﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VariantMeshEditor.ViewModels
{
    public class SceneElementHelper
    {
        public static List<T> GetAllOfTypeInSameVariantMesh<T>(FileSceneElement knownNode) where T : FileSceneElement
        {
            if (knownNode.Type != FileSceneElementEnum.VariantMesh)
            {
                knownNode = knownNode.Parent;
                while (knownNode != null)
                {
                    if (knownNode.Type == FileSceneElementEnum.VariantMesh)
                        break;

                    knownNode = knownNode.Parent;
                }
            }

            if (knownNode.Type != FileSceneElementEnum.VariantMesh)
                return new List<T>();

            var output = new List<T>();
            GetAllOfType(knownNode, ref output);
            return output;
        }


        static void GetAllOfType<T>(FileSceneElement root, ref List<T> outputList) where T : FileSceneElement
        {
            if (root as T != null)
                outputList.Add(root as T);

            foreach (var child in root.Children)
                GetAllOfType<T>(child, ref outputList);
        }

        public static void GetAllChildrenOfType<T>(FileSceneElement element, List<T> output) where T : FileSceneElement
        {
            if (element as T != null)
            {
                output.Add(element as T);
            }

            foreach (var child in element.Children)
            {
                GetAllChildrenOfType(child, output);
            }
        }


        public static void SetInitialVisability(FileSceneElement element, bool shouldBeSelected)
        {
            //element.PropertyChanged += Node_PropertyChanged;
            element.IsChecked = shouldBeSelected;

            if (element as AnimationElement != null)
                element.Vis = Visibility.Hidden;
            if (element as SkeletonElement != null)
                element.IsChecked = false;

            bool areAllChildrenModels = element.Children.Where(x => (x as RigidModelElement) != null).Count() == element.Children.Count();
            bool firstItem = true;
            foreach (var item in element.Children)
            {
                if (areAllChildrenModels && !firstItem)
                    shouldBeSelected = false;

                firstItem = false;
                SetInitialVisability(item, shouldBeSelected);
            }
        }
    }
}
