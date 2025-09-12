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
        console.error("Error loading post list", err);
      });
  }, []);

  useEffect(() => {
    reload();
  }, [reload]);

  return { posts, reload };
}
