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
    const base = import.meta.env.VITE_API_URL?.replace(/\/$/, "") ?? "";
    fetch(`${base}/api/posts`)
      .then((res) => res.json() as Promise<Post[]>)
      .then(setPosts)
      .catch((err) => {
        console.error("게시글 목록을 불러오는 중 오류", err);
      });
  }, []);

  useEffect(() => {
    reload();
  }, [reload]);

  return { posts, reload };
}
