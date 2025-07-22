import React, { useState, useEffect } from "react";
import redditPosts from "../redditPosts"
import Post from "./Post";
function Main(props) {
    const [lanes, setLanes] = useState([])
    function isSubRedditExist(){
        for(let i = 0 ; i<lanes.length ; i++){
            if(lanes[i].subredditName==props.subReddit){
                props.setRequestState("subReddit already exist")
                return true
            }
        }
        return false
    }
    async function FetchData() {
        if (props.subReddit && props.subReddit.trim() !== "") {
           
            try {
                let subReddit = (await redditPosts(props.subReddit));
                
                if (subReddit && subReddit !==undefined) {
                    props.setRequestState("subReddit Found ...")
                    if( isSubRedditExist()) return
                    setLanes((preValue) => [...preValue, subReddit])
                    props.setSubReddit("") // clean the state after sucess fetch
                }
                props.setRequestState("") // clean the request state also
            }
            catch(error){
                
                props.setRequestState("try another subReddit")
                return
            }
        }

    }

    function DeleteLane(event) {
        const laneIndex = event.target.getAttribute("data-index");
        setLanes((preValue) => {
            return preValue.filter((l, index) => index != laneIndex)
        })
    }

    useEffect(() => {
        FetchData()
    }, [props.subReddit])


    return <main>
        {lanes.map((l, index) => (
            <div key={index} className="lane" >
                 <header className="lane-header">
                    <h3>r/{l.subredditName}</h3>
                 </header>
                 <div className="posts-container">
                       {l.posts.map((p , index)=>(
                          <Post key = {index} post = {p} />
                       ))}
                       
                 </div>
                <button data-index={index} className="deleteLaneBtn" onClick={DeleteLane} >-</button>
            </div>
        ))}

    </main>

}



export default Main;