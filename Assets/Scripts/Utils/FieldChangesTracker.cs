
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace BionicWombat {
  public class FieldChangesTracker {
    Dictionary<string, string> lastValuesByFieldPath = new Dictionary<string, string>();

    public bool TrackFieldChanges<TOwner, TField>(TOwner rootOwnerInstance, Expression<Func<TOwner, TField>> fieldSelector)
        where TOwner : UnityEngine.Object {

      var fieldInfoPath = GetMemberInfoPath(rootOwnerInstance, fieldSelector);
      if (fieldInfoPath.Count == 0) {
        Debug.LogError("No member info path could be retrieved");
        return false;
      }

      FieldInfo fieldInfo = null;
      object targetObject = rootOwnerInstance;
      string fieldPath = null;

      for (int i = 0; i < fieldInfoPath.Count; i++) {
        if (fieldInfo != null)
          targetObject = targetObject != null ? fieldInfo.GetValue(targetObject) : null;

        fieldInfo = fieldInfoPath[i] as FieldInfo;
        if (fieldInfo == null) {
          Debug.LogError("One of the members in the field path is not a field");
          return false;
        }

        if (fieldInfo.GetCustomAttribute<SerializeReference>(true) != null) {
          Debug.LogError($"Fields with the {nameof(SerializeReference)} attribute are not supported for now");
          return false;
        }

        if (i > 0)
          fieldPath += ".";
        fieldPath += fieldInfo.Name;
      }

      if (targetObject == null) {
        // If the final target object is null, the owner instance may not have been initialized,
        // we call the method again after a delay to see if it's initialized then:
        UnityEditor.EditorApplication.delayCall += () => TrackFieldChanges(rootOwnerInstance, fieldSelector);
        return false;
      }

      object currentValueObject = fieldInfo.GetValue(targetObject);

      // If the current value object is null, the owner instance may not have been initialized.
      // We'll set a dummy value for UnityEngine.Object types, or will call the method again after a delay for other types,
      // otherwise in the next call the value will always be considered changed for several field types:
      if (currentValueObject == null) {
        Type fieldType = typeof(TField);

        if (fieldType == typeof(string)) {
          currentValueObject = string.Empty;
        } else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType)) {
          currentValueObject = "null";
        } else {
          UnityEditor.EditorApplication.delayCall += () => TrackFieldChanges(rootOwnerInstance, fieldSelector);
          return false;
        }
      }

      // Get the current value as a string:
      string currentValueString = null;

      if (currentValueObject != null) {
        if (currentValueObject is UnityEngine.Object) {
          currentValueString = currentValueObject.ToString();
        } else {
          try {
            currentValueString = JsonUtility.ToJson(currentValueObject);
          } catch (Exception) {
            Debug.LogError("Couldn't get the current value with \"JsonUtility.ToJson\"");
            return false;
          }

          if (string.IsNullOrEmpty(currentValueString) || currentValueString == "{}")
            currentValueString = currentValueObject.ToString();
        }
      }

      // Check if the value was changed, and store the current value:
      bool changed = lastValuesByFieldPath.TryGetValue(fieldPath, out string lastValue) && lastValue != currentValueString;

      lastValuesByFieldPath[fieldPath] = currentValueString;

      return changed;
    }

    public static List<MemberInfo> GetMemberInfoPath<TOwner, TMember>(TOwner ownerInstance, Expression<Func<TOwner, TMember>> memberSelector) {
      Expression body = memberSelector;
      if (body is LambdaExpression lambdaExpression) {
        body = lambdaExpression.Body;
      }

      List<MemberInfo> membersInfo = new List<MemberInfo>();
      while (body is MemberExpression memberExpression) {
        membersInfo.Add(memberExpression.Member);
        body = memberExpression.Expression;
      }

      membersInfo.Reverse();
      return membersInfo;
    }
  }


}
