import React, { useState, useEffect } from "react";
import Post from "./Post";
import {useLocation} from "react-router-dom"



function Main(props) {
    const [lanes, setLanes] = useState([])
    const [isLoading,setIsLoading] = useState(true)
    let location = useLocation()
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
        
            if( isSubRedditExist()) return //check if subReddit already exist,for better UI
            try {
                let response = await fetch(`/api/getLanes?userId=${props.user.id}`,{
                    method:"GET",
                    credentials:"include",
                   
                })
                if(response!=undefined && response.status!=200){
                    throw new Error("fetching error")
                }
                let json = await response.json()
                let subReddit = await json.lanes;
                if (subReddit && subReddit !==undefined) {
                    setLanes([...subReddit])
                    setIsLoading(false)
                }
                
            }
            catch(error){
                props.setRequestState(error.message)
                return
            }
        
        

    }

    async function DeleteLane(index,name) {
        
       
        try{
           let response = await fetch(`/api/deleteLane?userId=${props.user.id}&laneName=${name}`,{
            method:"GET",
            credentials:"include"
           }) 
           if(response.ok){
            setLanes(lanes.filter((_,i)=>i!=index))
           }
           else {
            props.setRequestState("deleting failed")
           }
        }

        catch(err){
            console.log(err)
        }
        
    }

    useEffect(() => {
        FetchData()
    }, [props.subReddit , location])


    if(isLoading){
        return (
          <div style={{
            height:"100vh",
            fontSize:"40px",
             display:"flex",  
             alignItems:"center" 
             ,justifyContent:"center"
            }
             }>
            loading ...
          </div>
        )
      }

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
                <button  className="deleteLaneBtn" onClick={()=>DeleteLane(index,l.subredditName)} >-</button>
            </div>
        ))}

    </main>

    }


export default Main;