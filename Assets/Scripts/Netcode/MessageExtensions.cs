using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Riptide;

public static class MessageExtensions
{
    #region Vector2
    /// <inheritdoc cref="AddVector2(Message, Vector2)"/>
    /// <remarks>This method is simply an alternative way of calling <see cref="AddVector2(Message, Vector2)"/>.</remarks>
    public static Message Add(this Message message, Vector2 value) => AddVector2(message, value);

    /// <summary>Adds a <see cref="Vector2"/> to the message.</summary>
    /// <param name="value">The <see cref="Vector2"/> to add.</param>
    /// <returns>The message that the <see cref="Vector2"/> was added to.</returns>
    public static Message AddVector2(this Message message, Vector2 value)
    {
        return message.AddFloat(value.x).AddFloat(value.y);
    }

    /// <summary>Retrieves a <see cref="Vector2"/> from the message.</summary>
    /// <returns>The <see cref="Vector2"/> that was retrieved.</returns>
    public static Vector2 GetVector2(this Message message)
    {
        return new Vector2(message.GetFloat(), message.GetFloat());
    }
    #endregion

    #region Vector3
    /// <inheritdoc cref="AddVector3(Message, Vector3)"/>
    /// <remarks>This method is simply an alternative way of calling <see cref="AddVector3(Message, Vector3)"/>.</remarks>
    public static Message Add(this Message message, Vector3 value) => AddVector3(message, value);

    /// <summary>Adds a <see cref="Vector3"/> to the message.</summary>
    /// <param name="value">The <see cref="Vector3"/> to add.</param>
    /// <returns>The message that the <see cref="Vector3"/> was added to.</returns>
    public static Message AddVector3(this Message message, Vector3 value)
    {
        return message.AddFloat(value.x).AddFloat(value.y).AddFloat(value.z);
    }

    /// <summary>Retrieves a <see cref="Vector3"/> from the message.</summary>
    /// <returns>The <see cref="Vector3"/> that was retrieved.</returns>
    public static Vector3 GetVector3(this Message message)
    {
        return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
    #endregion

    #region Quaternion
    /// <inheritdoc cref="AddQuaternion(Message, Quaternion)"/>
    /// <remarks>This method is simply an alternative way of calling <see cref="AddQuaternion(Message, Quaternion)"/>.</remarks>
    public static Message Add(this Message message, Quaternion value) => AddQuaternion(message, value);

    /// <summary>Adds a <see cref="Quaternion"/> to the message.</summary>
    /// <param name="value">The <see cref="Quaternion"/> to add.</param>
    /// <returns>The message that the <see cref="Quaternion"/> was added to.</returns>
    public static Message AddQuaternion(this Message message, Quaternion value)
    {
        return message.AddFloat(value.x).AddFloat(value.y).AddFloat(value.z).AddFloat(value.w);
    }

    /// <summary>Retrieves a <see cref="Quaternion"/> from the message.</summary>
    /// <returns>The <see cref="Quaternion"/> that was retrieved.</returns>
    public static Quaternion GetQuaternion(this Message message)
    {
        return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
    }

    public static Message AddLocalTransform(this Message message, LocalTransform value)
    {
        return message.AddVector3(value.Position).AddDOTSQuaternion(value.Rotation).AddFloat(value.Scale);
    }

    public static LocalTransform GetLocalTransform(this Message message)
    {
        return new LocalTransform() { Position = message.GetVector3(), Rotation = message.GetDOTSQuaternion(), Scale = message.GetFloat() };
    }

    public static Message AddDOTSQuaternion(this Message message, quaternion value)
    {
        return message.AddFloat4(value.value);
    }

    public static quaternion GetDOTSQuaternion(this Message message)
    {
        return new quaternion(message.GetFloat4());
    }

    public static Message AddFloat4(this Message message, float4 value)
    {
        return message.AddFloats(new float[] { value.w, value.x, value.y, value.z });
    }

    public static float4 GetFloat4(this Message message)
    {
        float[] floats = message.GetFloats();

        float4 float4 = new float4(floats[0], floats[1], floats[2], floats[3]);

        return float4;
    }

    #endregion
}