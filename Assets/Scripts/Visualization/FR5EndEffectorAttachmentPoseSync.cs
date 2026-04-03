// Folder: Visualization - shared helpers for end effector prefab alignment.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    public static class FR5EndEffectorAttachmentPoseSync
    {
        public static FR5EndEffectorAttachment FindAttachment(Transform root, string attachmentId = null)
        {
            if (root == null)
            {
                return null;
            }

            var attachments = root.GetComponentsInChildren<FR5EndEffectorAttachment>(true);
            for (var i = 0; i < attachments.Length; i++)
            {
                var attachment = attachments[i];
                if (attachment == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(attachmentId) || attachment.AttachmentId == attachmentId)
                {
                    return attachment;
                }
            }

            return null;
        }

        public static bool CopyPose(FR5EndEffectorAttachment source, FR5EndEffectorAttachment target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            source.EnsureStructure();
            target.EnsureStructure();

            CopyLocalTransform(source.transform, target.transform);
            CopyNamedChildren(source.transform, target.transform);
            target.SetTcpCalibrated(source.TcpCalibrated);
            return true;
        }

        private static void CopyNamedChildren(Transform sourceRoot, Transform targetRoot)
        {
            for (var i = 0; i < sourceRoot.childCount; i++)
            {
                var sourceChild = sourceRoot.GetChild(i);
                if (sourceChild == null)
                {
                    continue;
                }

                var targetChild = targetRoot.Find(sourceChild.name);
                if (targetChild == null)
                {
                    continue;
                }

                CopyLocalTransform(sourceChild, targetChild);
                CopyNamedChildren(sourceChild, targetChild);
            }
        }

        private static void CopyLocalTransform(Transform source, Transform target)
        {
            target.localPosition = source.localPosition;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }
    }
}
