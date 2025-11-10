using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye.VOT
{
    public enum ResponseStatus
    {
        Success = 0,

        ServerNotFound,
        Unauthorized,
        NetworkConnectionError,

        Failed,

        BadRequestServer,
        BadRequestClient,

        InternalServerError,

        UnknownError
    }
}