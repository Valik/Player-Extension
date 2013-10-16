using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension
{
    public enum VKScopeList
    {
        /// <summary>
        /// пользователь разрешил отправлять ему уведомления.
        /// </summary>
        NOTIFY = 1,
        /// <summary>
        /// доступ к друзьям.
        /// </summary>
        FRIENDS = 2,
        /// <summary>
        /// доступ к фотографиям.
        /// </summary>
        PHOTOS = 4,
        /// <summary>
        /// доступ к аудиозаписям.
        /// </summary>
        AUDIO = 8,
        /// <summary>
        /// доступ к видеозаписям.
        /// </summary>
        VIDEO = 16,
        /// <summary>
        /// доступ к предложениям (устаревшие методы).
        /// <summary>
        OFFERS = 32,
        /// <summary>
        /// доступ к вопросам (устаревшие методы).
        /// </summary>
        QUESTIONS = 64,
        /// <summary>
        /// доступ к wiki-страницам.
        /// </summary>
        PAGES = 128,
        /// <summary>
        /// добавление ссылки на приложение в меню слева.
        /// </summary>
        LINK = 256,
        /// <summary>
        /// доступ заметкам пользователя.
        /// </summary>
        NOTES = 2048,
        /// <summary>
        /// (для standalone-приложений) доступ к расширенным методам работы с сообщениями.
        /// </summary>
        MESSAGES = 4096,
        /// <summary>
        /// доступ к обычным и расширенным методам работы со стеной.
        /// </summary>
        WALL = 8192,
        /// <summary>
        /// доступ к документам пользователя.
        /// </summary>
        DOCS = 131072
    }
}
