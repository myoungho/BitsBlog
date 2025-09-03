import { Post } from "../hooks/usePosts";

interface PostsListProps {
  posts: Post[];
}

export function PostsList({ posts }: PostsListProps) {
  if (!posts.length) return <div>게시글이 없습니다.</div>;
  return (
    <div>
      {posts.map((p) => (
        <article key={p.id}>
          <h3>
            <a href={`/posts/${p.id}`}>{p.title}</a> ({new Date(p.created).toLocaleString()})
          </h3>
        </article>
      ))}
    </div>
  );
}

