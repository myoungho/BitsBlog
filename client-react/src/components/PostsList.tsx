import { Post } from "../hooks/usePosts";

interface PostsListProps {
  posts: Post[];
  onDelete?: (id: number) => void;
}

export function PostsList({ posts, onDelete }: PostsListProps) {
  if (!posts.length) return <div>게시글이 없습니다.</div>;
  return (
    <div>
      {posts.map((p) => (
        <article key={p.id}>
          <h3>
            {p.title} ({new Date(p.created).toLocaleString()})
          </h3>
          <p>{p.content}</p>
          <a href={`/edit/${p.id}`}>수정</a>
          {onDelete && (
            <button style={{ marginLeft: 8 }} onClick={() => onDelete(p.id)}>
              삭제
            </button>
          )}
        </article>
      ))}
    </div>
  );
}
