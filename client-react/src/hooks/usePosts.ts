/// <reference types="vite/client" />
import { useCallback, useEffect, useState } from "react";

export interface Post {
  id: number;
  title: string;
  content: string;
  created: string;
}

export function usePosts() {
  const [posts, setPosts] = useState<Post[]>([]);

  const reload = useCallback(() => {
    fetch(`${import.meta.env.VITE_API_URL}/api/posts`)
      .then((res) => res.json() as Promise<Post[]>)
      .then(setPosts)
      .catch((err) => {
        console.error("게시글 목록 불러오기 실패", err);
      });
  }, []);

  useEffect(() => {
    reload();
  }, [reload]);

    return { posts, reload };
}

