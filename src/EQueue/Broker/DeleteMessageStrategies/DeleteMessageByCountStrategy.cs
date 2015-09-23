﻿using System.Collections.Generic;
using System.Linq;
using ECommon.Utilities;
using EQueue.Broker.Storage;

namespace EQueue.Broker.DeleteMessageStrategies
{
    public class DeleteMessageByCountStrategy : IDeleteMessageStrategy
    {
        /// <summary>表示磁盘上可以保存的最多的Chunk文件的个数；
        /// <remarks>
        /// 比如设置为100，则磁盘上可以保存的最多的Chunk文件的个数为100，如果现在总的个数超过100，则最先产生的Chunk文件就会被删除。
        /// 默认值为200，即如果每个Chunk文件的大小为256MB的话，则200 * 256 = 50GB，即磁盘总共会保存最多默认50GB的消息。
        /// </remarks>
        /// </summary>
        public int MaxChunkCount { get; private set; }

        public DeleteMessageByCountStrategy(int maxChunkCount = 200)
        {
            Ensure.Positive(maxChunkCount, "maxChunkCount");
            MaxChunkCount = maxChunkCount;
        }

        public IEnumerable<TFChunk> GetAllowDeleteChunks(TFChunkManager chunkManager, long maxMessagePosition)
        {
            var chunks = new List<TFChunk>();
            var allCompletedChunks = chunkManager
                .GetAllChunks()
                .Where(x => x.IsCompleted && x.ChunkHeader.ChunkDataEndPosition <= maxMessagePosition)
                .ToList();

            var exceedCount = allCompletedChunks.Count - MaxChunkCount;
            if (exceedCount <= 0)
            {
                return chunks;
            }

            for (var i = 0; i < exceedCount; i++)
            {
                chunks.Add(allCompletedChunks[i]);
            }

            return chunks;
        }
    }
}
